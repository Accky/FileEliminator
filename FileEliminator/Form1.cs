using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using IniParser;
using IniParser.Model;

namespace FileEliminator
{
    public partial class Form1 : Form
    {
        public class Parameters
        {
            public string Path = "";
            public List<string> Rules = new List<string>();
        }
        
        private Parameters parameter = new Parameters();
        const string settingFileName = "settings.ini";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var parser = new IniParser.FileIniDataParser();
            var data = parser.ReadFile(settingFileName);
            
            //読み込み
            parameter.Path = data["Settings"]["Path"];
            string rules = data["Settings"]["Rules"];
            parameter.Rules = rules.Split(',').ToList();
            parameter.Rules.RemoveAll(x => x.Length == 0);

            textBox1.Text = parameter.Path;
            listBox1.Items.AddRange(parameter.Rules.ToArray());
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            var parser = new IniParser.FileIniDataParser();
            var data = parser.ReadFile(settingFileName);

            //書き込み
            data["Settings"]["Path"] = parameter.Path;
            string rules = "";
            foreach (var str in parameter.Rules) {
                rules += str + ",";
            }
            data["Settings"]["Rules"] = rules;

            parser.WriteFile(settingFileName, data);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //FolderBrowserDialogクラスのインスタンスを作成
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            //上部に表示する説明テキストを指定する
            fbd.Description = "フォルダを指定してください。";
            //ルートフォルダを指定する
            //デフォルトでDesktop
            fbd.RootFolder = Environment.SpecialFolder.Desktop;
            //最初に選択するフォルダを指定する
            //RootFolder以下にあるフォルダである必要がある
            if (parameter.Path.Length > 0) {
                fbd.SelectedPath = parameter.Path;
            }
            else {
                fbd.SelectedPath = @"C:\Windows";
                parameter.Path = fbd.SelectedPath;
            }

            //ユーザーが新しいフォルダを作成できるようにする
            //デフォルトでTrue
            fbd.ShowNewFolderButton = true;

            //ダイアログを表示する
            if (fbd.ShowDialog(this) == DialogResult.OK) {

                parameter.Path = fbd.SelectedPath;
                textBox1.Text = parameter.Path;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            listBox1.Items.Add(textBox2.Text);
            textBox2.Text = "";

            parameter.Rules.Clear();
            foreach (string strCol in listBox1.Items) {
                parameter.Rules.Add(strCol);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var select = listBox1.SelectedItem;
            listBox1.Items.Remove(select);

            parameter.Rules.Clear();
            foreach (string strCol in listBox1.Items) {
                parameter.Rules.Add(strCol);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            List<string> pathList = new List<string>();

            foreach (var rule in parameter.Rules) {
                string[] files = Directory.GetFiles(parameter.Path, rule, SearchOption.AllDirectories);
                string[] folders = System.IO.Directory.GetDirectories(parameter.Path, rule, SearchOption.AllDirectories);
                pathList.AddRange(files);
                pathList.AddRange(folders);
            }

            pathList.Sort();

            listBox2.Items.Clear();
            listBox2.Items.AddRange(pathList.ToArray());
        }

        private void button5_Click(object sender, EventArgs e)
        {
            foreach (string path in listBox2.Items) {
                if (File.Exists(path)) {
                    File.Delete(path);
                }
                else if (Directory.Exists(path)) {
                    DeleteAll(path);
                }
            }

            listBox2.Items.Clear();
        }

        //フォルダの中身を強制的に削除
        public void DeleteAll(string targetDirectoryPath)
        {
            if (!Directory.Exists(targetDirectoryPath)) {
                return;
            }

            //ディレクトリ以外の全ファイルを削除
            string[] filePaths = Directory.GetFiles(targetDirectoryPath);
            foreach (string filePath in filePaths) {
                File.SetAttributes(filePath, FileAttributes.Normal);
                File.Delete(filePath);
            }

            //ディレクトリの中のディレクトリも再帰的に削除
            string[] directoryPaths = Directory.GetDirectories(targetDirectoryPath);
            foreach (string directoryPath in directoryPaths) {
                DeleteAll(directoryPath);
            }

            //中が空になったらディレクトリ自身も削除
            Directory.Delete(targetDirectoryPath, false);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            parameter.Path = textBox1.Text;
        }

        private void textBox2_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) {
                listBox1.Items.Add(textBox2.Text);
                textBox2.Text = "";

                parameter.Rules.Clear();
                foreach (string strCol in listBox1.Items) {
                    parameter.Rules.Add(strCol);
                }
            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            var select = listBox1.SelectedItem;

            textBox2.Text = select as string;

            listBox1.Items.Remove(select);

            parameter.Rules.Clear();
            foreach (string strCol in listBox1.Items) {
                parameter.Rules.Add(strCol);
            }

            textBox2.Focus();
        }
    }
}
