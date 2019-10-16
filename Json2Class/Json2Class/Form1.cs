using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonFomatter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            
            checkBox3.Checked = true;
            checkBox1.Checked = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var originalValue = textBox1.Text;
            string str = "public class RootObject\n{\r\n";

            var ret = GetClassLevel(str, originalValue, 0, true);
            ret.Sort();
            textBox2.Text = "";
            for (var i = 0; i < ret.Count; i++)
            {
                textBox2.Text += "\r\n" + ret[i].Item2;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="NewStr">递归str</param>
        /// <param name="orgStr">json对象</param>
        /// <param name="count">递归层数</param>
        /// <param name="isFirstLevel">是否是第一层</param>
        /// <returns></returns>
        private List<Tuple<int, string>> GetClassLevel(string NewStr, string orgStr, int count, bool isFirstLevel)
        {
            List<Tuple<int, string>> orgdic = new List<Tuple<int, string>>();
            try
            {
                var obj = JsonConvert.DeserializeObject<JObject>(orgStr);
                foreach (var item in obj)
                {
                    var key = item.Key;
                    var value = item.Value;
                    if (value.HasValues)//为对象
                    {
                        if (value.Type is JTokenType.Array)//数组
                        {
                            if (value.LastOrDefault().Type is JTokenType.Integer)
                            {
                                NewStr += GetJsonProperty(key);
                                NewStr += "\tpublic List<int> " + GetProperName(key) + " {get;set;}\r\n";
                            }
                            else if (value.LastOrDefault().Type is JTokenType.String)
                            {
                                NewStr += GetJsonProperty(key);
                                NewStr += "\tpublic List<string> " + GetProperName(key) + " {get;set;}\r\n";
                            }
                        }
                        else if (value.Type is JTokenType.Object)
                        {
                            //递归
                            NewStr += GetJsonProperty(key);
                            NewStr += "\tpublic " + GetProperName(key) + " " + key.ToLower() + " {get;set;}\r\n";//类的属性名采用小写
                            var ret = GetClassLevel("public class " + GetProperName(key) + " \n{\r\n", JsonConvert.SerializeObject(value), ++count, false);
                            foreach (var val in ret)
                            {
                                orgdic.Add(Tuple.Create(val.Item1, val.Item2));
                            }
                            // count = 0;
                        }
                    }
                    else//为值
                    {
                        if (value.Type is JTokenType.String)
                        {
                            NewStr += GetJsonProperty(key);
                            NewStr += "\tpublic string " + GetProperName(key) + " {get;set;}\r\n";
                        }
                        else if (value.Type is JTokenType.Integer)
                        {
                            NewStr += GetJsonProperty(key);
                            NewStr += "\tpublic int " + GetProperName(key) + " {get;set;}\r\n";
                        }
                        else if (value.Type is JTokenType.Date)
                        {
                            NewStr += GetJsonProperty(key);
                            NewStr += "\tpublic DateTime " + GetProperName(key) + " {get;set;}\r\n";
                        }
                        else if (value.Type is JTokenType.Guid)
                        {
                            NewStr += GetJsonProperty(key);
                            NewStr += "\tpublic Guid " + GetProperName(key) + " {get;set;}\r\n";
                        }
                        else if (value.Type is JTokenType.Boolean)
                        {
                            NewStr += GetJsonProperty(key);
                            NewStr += "\tpublic bool " + GetProperName(key) + " {get;set;}\r\n";
                        }
                    }
                }
                NewStr += "}\r\n";
                if (isFirstLevel)
                {
                    orgdic.Add(Tuple.Create(0, NewStr));
                }
                else
                {
                    orgdic.Add(Tuple.Create(count, NewStr));
                }
            }
            catch (Exception e)
            {
                textBox2.Text = e.Message;
            }
            return orgdic;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetProperName(string key)
        {
            string ProperName = key;

            if (checkBox3.Checked)//Pascal
            {
                ProperName = "";
                if (key.IndexOf("_") > 0)//下划线命名
                {
                    var NameStr = key.Split('_');
                    for (var i = 0; i < NameStr.Length; i++)
                    {
                        ProperName += Char.ToUpper(NameStr[i][0]) + NameStr[i].Substring(1);
                    }
                }
                else//驼峰式
                {
                    ProperName += Char.ToUpper(key[0]) + key.Substring(1);
                }
            }
            if (checkBox4.Checked)//Camel
            {
                ProperName = "";
                if (key.IndexOf("_") > 0)//下划线命名
                {
                    var NameStr = key.Split('_');
                    for (var i = 0; i < NameStr.Length; i++)
                    {
                        if (i!=0)
                        {
                            ProperName += Char.ToUpper(NameStr[i][0]) + NameStr[i].Substring(1);
                        }
                        else
                        {
                            ProperName += Char.ToLower(NameStr[i][0]) + NameStr[i].Substring(1);
                        }
                    }
                }
                else//驼峰式
                {
                    ProperName += Char.ToLower(key[0]) + key.Substring(1);
                }
            }
            return ProperName;
        }
        private string GetJsonProperty(string key)
        {
            string propertystr = string.Empty;
            bool flag = false;
            if (checkBox1.Checked)
            {
                flag = true;
                propertystr += "\"" + key + "\",";
            }
            if (checkBox2.Checked)
            {
                flag = true;
                propertystr += "NullValueHandling=NullValueHandling.Ignore,";
            }
            if (flag)
            {
                propertystr = "\t[JsonProperty(" + propertystr.TrimEnd(',') + ")]\r\n";
            }
            return propertystr;
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked)
            {
                checkBox3.Checked = false;
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                checkBox4.Checked = false;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
