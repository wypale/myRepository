using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Xml;

namespace mathProject
{
    internal class ClassUserCadNums
    {
        // private XmlDocument docUserKadNums = new XmlDocument();
        private List<DataBase> AllDataBase = new List<DataBase>();
        private string userNumsFile = ".\\data\\userNums.xml";
        private string Directory = "";
        // private XmlNode rootNode;
        private System.Timers.Timer aTimer = null;
        private bool timerWork = false;
        private bool timerWorkDownload = false;
        private double StatusUpdateInterval = 1.0 * 60 * 1000;
        private double DownloadUpdateInterval = 1000;
        private MyStatus Status = new MyStatus();






        internal ClassUserCadNums(DataGridView grid)
        {

            LoadDataGridView(grid);
            if (aTimer == null)
            {
                System.Threading.Thread thr = new System.Threading.Thread(p => GetStatus());
                thr.Start();
                aTimer = new System.Timers.Timer(StatusUpdateInterval);
                aTimer.Enabled = true;
                //    aTimer.Elapsed += new ElapsedEventHandler(delegate(object sender, ElapsedEventArgs e) { GetStatus(); });
                //  aTimer.Elapsed += new ElapsedEventHandler((object sender, ElapsedEventArgs e)=> GetStatus());
                aTimer.Elapsed += new ElapsedEventHandler((p, c) => GetStatus());
                aTimer.Start();
            }
        }





        private void GetStatus()
        {
            if (timerWork)
            { }
            else
            {
                Dictionary<string, DataBase> sendedList = new Dictionary<string, DataBase>();
                timerWork = true;
                foreach (DataBase nodeBase in AllDataBase)
                {
                    MyXmlNode node = nodeBase.node;
                    bool started = node.started;
                    bool finished = node.finished;
                    bool downloaded = node.downloaded;
                    string number = node.number;
                    if (started && !finished && !downloaded)
                        sendedList.Add(ClassCalcConnect.GetChangeNumbStr(number), nodeBase);
                }
                if (sendedList.Count > 0)
                {
                    int[] status = CppClass.GetStatusFiles(sendedList.Keys.ToArray());
                    for (int i = 0; i < sendedList.Count; i++)
                    {
                        var item = sendedList.ElementAt(i);
                        item.Value.node.curStatus = status[i];
                        updateRowByStatus(item.Value);
                    }

                }
                timerWork = false;
            }
        }



        private void updateRowsByStatus()
        {
            foreach (DataBase nodeBase in AllDataBase)
            {
                updateRowByStatus(nodeBase);

            }
        }
        private void updateRowByStatus(DataBase nodeBase)
        {
            MyXmlNode node = nodeBase.node;
            bool started = node.started;
            bool finished = node.finished;
            bool downloaded = node.downloaded;
            //  string number = node.number;
            int status = node.curStatus;
            DataGridViewCell cell = nodeBase.row.Cells[2];
            DataGridViewCellStyle style = nodeBase.row.DataGridView.DefaultCellStyle;
            nodeBase.row.DataGridView.Invoke(new Action(() =>
            {

                if (started && downloaded && !finished)
                {
                    //прогресс загрузки curStatus
                    //nodeBase.row.Cells[2].Style.BackColor = Color.White;
                    nodeBase.row.Cells[2].Style.BackColor = Color.White;
                    nodeBase.row.Cells[2].Value = new object[] { "downloaded", nodeBase.node.curStatus };// 

                    // updateProgressCell(cell, style, Color.Red, "downloaded", 10);
                }
                else if (started && !downloaded && finished)
                {
                    //файл скачан и обработан
                    nodeBase.row.Cells[2].Style.BackColor = Color.LawnGreen;
                    nodeBase.row.Cells[2].Value = new object[] { "finished", -1 };// 

                    //  updateProgressCell(cell, style, Color.Red, "finished", -1);
                }
                else if (started && !downloaded && !finished)
                {
                    nodeBase.row.Cells[2].Style.BackColor = Color.LightGreen;
                    nodeBase.row.Cells[2].Value = new object[] { "started", -1 };//
                    //   updateProgressCell(cell, style, Color.Red, "started", -1);
                    //статус по curStatus
                }
                // else
                //     nodeBase.row.Cells[2].Value =  new object[] { " ", -1 };// "started";
            }));
        }





        internal void LoadDataGridView(DataGridView grid)
        {
            XmlDocument docUserKadNums = new XmlDocument();
            if (!System.IO.File.Exists(userNumsFile))
            {
                XmlElement root = docUserKadNums.CreateElement("CadNums");
                docUserKadNums.AppendChild(root);
            }
            else
                docUserKadNums.Load(userNumsFile);
            XmlNode rootNode = docUserKadNums.DocumentElement;
            foreach (XmlNode node in rootNode.ChildNodes)
            {
                if (node.Name != "userNum") continue;
                grid.Rows.Add();
                DataGridViewRow row = grid.Rows[grid.Rows.Count - 1];
                DataBase baseCur = new DataBase();

                //node.Attributes["downloaded"].Value = (false).ToString();
                baseCur.node.downloaded = false;
                baseCur.node.ischecked = Convert.ToBoolean(node.Attributes["checked"].Value);
                baseCur.node.curStatus = Convert.ToInt32(node.Attributes["curStatus"].Value);
                baseCur.node.finished = Convert.ToBoolean(node.Attributes["finished"].Value);
                baseCur.node.number = node.Attributes["number"].Value;
                baseCur.node.started = Convert.ToBoolean(node.Attributes["started"].Value);

                row.Cells[0].Value = baseCur.node.ischecked;
                row.Cells[1].Value = baseCur.node.number;
                baseCur.row = row;
                AllDataBase.Add(baseCur);


                row.Cells[2].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                //  if (started && !finished&&status != -1)
                //  {
                //      MyStat stat=Status.getStatByCode(status);
                //      row.Cells[2].Style.BackColor = stat.statColor;
                //      row.Cells[2].Value = stat.statString;
                //   }
                //   if (started && !finished)
                //    {
                //        row.Cells[2].Style.BackColor = Color.Yellow;
                //        row.Cells[2].Value = "В обработке";
                //    }
            }
            XmlNode dir = rootNode.SelectSingleNode("./Directory");
            bool NewNode = false;
            if (dir == null)
                NewNode = true;
            else if (dir.InnerText == "")
                NewNode = true;

            if (NewNode)
            {
                Directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\";

            }
            else
                Directory = dir.InnerText;
        }

        internal void SavePathDirectory(string dir)
        {
            Directory = dir;
        }

        internal void Save(DataGridView grid)
        {
            XmlDocument docUserKadNums = new XmlDocument();
            if (!System.IO.File.Exists(userNumsFile))
            {
                XmlElement root = docUserKadNums.CreateElement("CadNums");
                docUserKadNums.AppendChild(root);
            }
            else
                docUserKadNums.Load(userNumsFile);
            XmlNode rootNode = docUserKadNums.DocumentElement;
            rootNode.RemoveAll();


            for (int i = 0; i < AllDataBase.Count; i++)
            {
                MyXmlNode mynode = AllDataBase[i].node;
                XmlNode node = docUserKadNums.CreateElement("userNum");

                XmlAttribute selected = docUserKadNums.CreateAttribute("checked");
                selected.Value = AllDataBase[i].row.Cells[0].Value.ToString();

                XmlAttribute number = docUserKadNums.CreateAttribute("number");
                number.Value = mynode.number.ToString();

                XmlAttribute started = docUserKadNums.CreateAttribute("started");
                started.Value = mynode.started.ToString();

                XmlAttribute curStatus = docUserKadNums.CreateAttribute("curStatus");
                curStatus.Value = mynode.curStatus.ToString();

                XmlAttribute finished = docUserKadNums.CreateAttribute("finished");
                finished.Value = mynode.finished.ToString();

                XmlAttribute downloaded = docUserKadNums.CreateAttribute("downloaded");
                downloaded.Value = mynode.downloaded.ToString();

                node.Attributes.Append(selected);
                node.Attributes.Append(number);
                node.Attributes.Append(started);
                node.Attributes.Append(curStatus);
                node.Attributes.Append(finished);
                node.Attributes.Append(downloaded);

                rootNode.AppendChild(node);
            }
            XmlNode dir = rootNode.SelectSingleNode("./Directory");
            if (dir == null)
            {
                dir = docUserKadNums.CreateElement("Directory");
                rootNode.AppendChild(dir);
            }
            dir.InnerText = Directory;
            docUserKadNums.Save(userNumsFile);
            aTimer.Stop();
            aTimer.Dispose();
        }

        internal void RemoveByIndexes(int[] indexes)
        {
            foreach (int ind in indexes)
                RemoveByIndex(ind);
        }

        internal void RemoveByIndex(int index)
        {
            AllDataBase.RemoveAt(index);
            // rootNode.RemoveChild(rootNode.ChildNodes[index]);
        }
        internal void RemoveAll()
        {
            AllDataBase.Clear();
            if (System.IO.File.Exists(userNumsFile))
                System.IO.File.Delete(userNumsFile);
        }

        internal void AddByIndex(DataGridViewRow row)
        {
            DataBase baseCur = new DataBase();

            int AddedIndex = row.Index;
            MyXmlNode node = new MyXmlNode();
            node.ischecked = (bool)row.Cells[0].Value;
            node.number = row.Cells[1].Value.ToString();
            node.started = false;
            node.curStatus = -1;
            node.finished = false;
            node.downloaded = false;
            baseCur.row = row;
            baseCur.node = node;
            AllDataBase.Add(baseCur);

        }

        internal void StartToSend(List<int> indexes)
        {
            List<string> cadNums = new List<string>();
            foreach (int i in indexes)
            {
                MyXmlNode node = AllDataBase[i].node;
                bool downloaded = node.downloaded;

                if (!downloaded)
                {
                    cadNums.Add(ClassCalcConnect.GetChangeNumbStr(node.number));
                    node.started = true;
                    node.finished = false;
                    node.downloaded = false;
                    node.curStatus = -1;
                }
            }

            System.Threading.Thread thr = new System.Threading.Thread(() => onSendNums(cadNums));
            thr.Start();
        }

        private void onSendNums(List<string> cadNums)
        {
            CppClass.StartCreateFilesOnServer(cadNums.ToArray());
            // Thread.Sleep(10000);
            GetStatus();
            //throw new NotImplementedException();
        }








        private bool downloadedStarted = false;
        internal void StartToDownload(List<int> indexes)
        {

            List<string> cadNums = new List<string>();
            foreach (int i in indexes)
            {
                MyXmlNode node = AllDataBase[i].node;
                bool started = node.started;
                bool downloaded = node.downloaded;
                if (started && !downloaded)
                {
                    node.started = true;
                    node.finished = false;
                    node.curStatus = -1;
                    node.downloaded = true;
                    cadNums.Add(ClassCalcConnect.GetChangeNumbStr(node.number));
                }
            }
            if (cadNums.Count == 0) return;
            if (!downloadedStarted)
            {
                System.Threading.Thread thr = new System.Threading.Thread(() => onSendNumsDownLoad(cadNums));
                thr.Start();
            }
            else
            {
                CppClass.DownloadFilesFromServer(cadNums.ToArray());
                System.Threading.Thread thr = new System.Threading.Thread(p => updateDownloadRows());
                thr.Start();
            }
        }

        private System.Timers.Timer aTimerDownload;
        private void onSendNumsDownLoad(List<string> cadNums)
        {

            downloadedStarted = true;
            CppClass.DownloadFilesFromServer(cadNums.ToArray());

            System.Threading.Thread thr = new System.Threading.Thread(p => updateDownloadRows());
            thr.Start();

            aTimerDownload = new System.Timers.Timer(DownloadUpdateInterval);
            aTimerDownload.Enabled = true;
            //    aTimer.Elapsed += new ElapsedEventHandler(delegate(object sender, ElapsedEventArgs e) { GetStatus(); });
            //  aTimer.Elapsed += new ElapsedEventHandler((object sender, ElapsedEventArgs e)=> GetStatus());
            aTimerDownload.Elapsed += new ElapsedEventHandler((p, c) => updateDownloadRows());
            aTimerDownload.Start();
            // Thread.Sleep(10000);

            //throw new NotImplementedException();
        }
        private void updateDownloadRows()
        {
            if (timerWorkDownload)
            { }
            else
            {
                Dictionary<string, DataBase> sendedList = new Dictionary<string, DataBase>();
                timerWorkDownload = true;
                foreach (DataBase nodeBase in AllDataBase)
                {
                    MyXmlNode node = nodeBase.node;
                    bool started = node.started;
                    bool finished = node.finished;
                    bool downloaded = node.downloaded;
                    string number = node.number;
                    if (started && !finished && downloaded)
                        sendedList.Add(number, nodeBase);

                }
                if (sendedList.Count > 0)
                {
                    for (int i = 0; i < sendedList.Count; i++)
                    {
                        var item = sendedList.ElementAt(i);
                        point_c progress = CppClass.GetProgressFile_(item.Key);
                        item.Value.node.curStatus = (int)progress.X_B;
                        if (progress.Y_L == 1)
                        {

                            string stringForParse = CppClass.GetFile_(item.Key);
                            updateRowByStatus(item.Value);
                            //TODO парсим файл                       

                            item.Value.node.finished = true;
                            item.Value.node.curStatus = -1;
                            item.Value.node.downloaded = false;
                        }
                        updateRowByStatus(item.Value);
                    }
                }
                else
                {
                    downloadedStarted = false;
                    aTimerDownload.Stop();
                    aTimerDownload = null;
                }
                timerWorkDownload = false;
            }
        }

        internal void StopDownload()
        {
            System.Threading.Thread thr = new System.Threading.Thread(p =>
            {
                if (downloadedStarted)
                {
                    downloadedStarted = false;
                    aTimerDownload.Stop();
                    aTimerDownload = null;
                    timerWorkDownload = false;
                    CppClass.StopDownloadFilesFromServer_();
                    foreach (DataBase nodeBase in AllDataBase)
                    {
                        nodeBase.node.downloaded = false;
                    }
                    GetStatus();
                }
            });
            thr.Start();
        }


        private struct MyStat
        {
            internal int statCode;
            internal Color statColor;
            internal string statString;
            public MyStat(int statCode, Color statColor, string statString)
            {
                this.statCode = statCode;
                this.statColor = statColor;
                this.statString = statString;
            }
        }

        private class MyStatus
        {
            internal List<MyStat> Status = new List<MyStat>();
            internal MyStatus()
            {
                MyStat inProcess = new MyStat(1, Color.Yellow, "В обработке");
                MyStat inError = new MyStat(2, Color.Red, "Ошибка получения");
                MyStat inFinished = new MyStat(3, Color.LightGreen, "Сформировано");
                MyStat inError1 = new MyStat(4, Color.Red, "Ошибка получения");
                Status.Add(inProcess);
                Status.Add(inError);
                Status.Add(inFinished);
                Status.Add(inError1);
            }
            internal MyStat getStatByCode(int code)
            {
                return Status.Find(p => p.statCode == code);
            }
        }


        public string GetPathDirectory()
        {
            return Directory;
        }

        //public void SavePathDirectory(string path)
        // {


        //}


        private class DataBase
        {
            internal MyXmlNode node = new MyXmlNode();
            internal DataGridViewRow row = null;
        }

        private class MyXmlNode
        {
            internal bool ischecked = false;
            internal string number = "";
            internal bool started = false;
            internal int curStatus = 0;
            internal bool finished = false;
            internal bool downloaded = false;
        }


        private class CppClass
        {
            [DllImport("DinamicLibrary.dll")]
            private static extern IntPtr GetFile(string intro);
            [DllImport("DinamicLibrary.dll")]
            private static extern void DelFile(string intro);
            internal static string GetFile_(string kadNum)
            {

                IntPtr ptr = GetFile(kadNum);
                string Numb = Marshal.PtrToStringAnsi(ptr);
                DelFile(kadNum);
                int asdsdfdsf = Numb.Length;
                return Numb;
            }

            [DllImport("DinamicLibrary.dll")]
            private static extern void DownloadFilesFromServer(string[] str, int length);
            internal static void DownloadFilesFromServer(string[] kadNums)
            {
                DownloadFilesFromServer(kadNums, kadNums.Length);
            }


            [DllImport("DinamicLibrary.dll")]
            private static extern point_c GetProgressFile(string kadNum);
            internal static point_c GetProgressFile_(string kadNum)
            {
                return GetProgressFile(kadNum);
            }

            [DllImport("DinamicLibrary.dll")]
            private static extern IntPtr GetStatusFiles(string[] kadNums, int length);//,out IntPtr statuses);
            [DllImport("DinamicLibrary.dll")]
            private static extern int DelStatusFiles(IntPtr statuses);
            internal static int[] GetStatusFiles(string[] kadNums)
            {
                int lenght = kadNums.Length;
                //  IntPtr IntegerArrayReceiver = IntPtr.Zero;
                int[] ret = new int[lenght];
                IntPtr IntegerArrayReceiver = GetStatusFiles(kadNums, lenght);//, out IntegerArrayReceiver);
                Marshal.Copy(IntegerArrayReceiver, ret, 0, lenght);
                DelStatusFiles(IntegerArrayReceiver);
                return ret;
            }

            [DllImport("DinamicLibrary.dll")]
            private static extern void StartCreateFilesOnServer(string[] kadNums, int length);
            internal static void StartCreateFilesOnServer(string[] kadNums)
            {
                StartCreateFilesOnServer(kadNums, kadNums.Length);
            }

            [DllImport("DinamicLibrary.dll")]
            private static extern void StopDownloadFilesFromServer();
            internal static void StopDownloadFilesFromServer_()
            {
                StopDownloadFilesFromServer();

            }
        }

    }
}
