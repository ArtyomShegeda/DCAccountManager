using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace DragonCityAccountsManager
{
    public partial class Form1 : Form
    {
        private static string SettingsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DragonCityAccountsManager");
        private static string SettingsFilePath = Path.Combine(SettingsFolder, "accounts.xml");
        Dictionary<string, dynamic> accounts = new Dictionary<string, dynamic>();

        public Form1()
        {
            InitializeComponent();

            LoadAccounts();
            DetectNewAccount();
            RenderAccounts();
        }

        private void DetectNewAccount()
        {
            var roamingPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Social Point", "DragonCity");
            var localPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Social Point", "DragonCity");

            if (File.Exists(Path.Combine(roamingPath, "folderAttrStorage-userId.json")) 
                && File.Exists(Path.Combine(roamingPath, "folderAttrStorage-securityToken.json"))
                && File.Exists(Path.Combine(localPath, "folderAttrStorage-userId.json"))
                && File.Exists(Path.Combine(localPath, "folderAttrStorage-securityToken.json")))
            {
                var id = File.ReadAllText(Path.Combine(localPath, "folderAttrStorage-userId.json"));
                var securityToken = File.ReadAllText(Path.Combine(localPath, "folderAttrStorage-securityToken.json"));

                if (!accounts.ContainsKey(id))
                    accounts.Add(id, CreateAccount(id, id, securityToken));

                SaveAccounts();
            }
            else
            {
                MessageBox.Show("Sorry.\nCannot detect Dragon City game settings.");
            }
        }

        private void RenderAccounts()
        {
            foreach (var account in accounts.Values)
            {
                if (listView1.Items[account.id] == null)
                {
                    listView1.Items.Add(new ListViewItem(new string[] { account.name, account.id })
                    {
                        Name = account.id
                    });
                }
            }
        }

        private void LoadAccounts()
        {
            if (!File.Exists(SettingsFilePath))
                return;

            XmlDocument doc = new XmlDocument();
            doc.Load(SettingsFilePath);

            foreach(XmlNode node in doc.DocumentElement.ChildNodes)
            {
                var account = CreateAccount(
                    node.Attributes["id"].Value, 
                    node.Attributes["name"].Value,
                    node.Attributes["securityToken"].Value);
                accounts.Add(account.id, account);
            }
        }

        private dynamic CreateAccount(string id, string name, string securityToken)
        {
            return new
            {
                name,
                id,
                securityToken
            };
        }

        private void SaveAccounts()
        {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateElement("accounts"));

            foreach (var account in accounts.Values)
            {
                var node = doc.CreateElement("account");
                var attribute = doc.CreateAttribute("name");
                attribute.Value = account.name;
                node.Attributes.Append(attribute);
                attribute = doc.CreateAttribute("id");
                attribute.Value = account.id;
                node.Attributes.Append(attribute);
                attribute = doc.CreateAttribute("securityToken");
                attribute.Value = account.securityToken;
                node.Attributes.Append(attribute);
                doc.DocumentElement.AppendChild(node);
            }

            Directory.CreateDirectory(SettingsFolder);
            doc.Save(SettingsFilePath);
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count != 1)
                return;
            var id = listView1.SelectedItems[0].Name;
            if (accounts[id].name != textBox1.Text) {
                accounts[id] = CreateAccount(id, textBox1.Text, accounts[id].securityToken);
                listView1.SelectedItems[0].Text = textBox1.Text;
                SaveAccounts();
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                textBox1.Text = string.Empty;
                textBox2.Text = string.Empty;
                button1.Enabled = false;
                return;
            }
            if (listView1.SelectedItems.Count != 1)
                return;

            var id = listView1.SelectedItems[0].Name;
            textBox1.Text = accounts[id].name;
            textBox2.Text = id;
            button1.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var roamingPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Social Point", "DragonCity");
            var localPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Social Point", "DragonCity");

            var id = listView1.SelectedItems[0].Name;
            var account = accounts[id];
            File.WriteAllText(Path.Combine(localPath, "folderAttrStorage-userId.json"), id);
            File.WriteAllText(Path.Combine(localPath, "folderAttrStorage-securityToken.json"), account.securityToken);
            File.WriteAllText(Path.Combine(roamingPath, "folderAttrStorage-userId.json"), id);
            File.WriteAllText(Path.Combine(roamingPath, "folderAttrStorage-securityToken.json"), account.securityToken);
        }
    }
}
