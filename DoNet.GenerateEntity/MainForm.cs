using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DoNet.GenerateEntity.SqlServer;

namespace DoNet.GenerateEntity
{
    public partial class MainForm : Form
    {
        #region 全局变量

        private string _lastSelectFolder = null;

        #endregion

        #region 页面事件

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            ShowDataTables();
            ShowNameSpaceList();
        }

        private void btnManageNameSpace_Click(object sender, EventArgs e)
        {
            NameSpaceManager nsm = new NameSpaceManager();
            nsm.ShowDialog();
            ShowNameSpaceList();
        }

        private void btnCreateCodeFile_Click(object sender, EventArgs e)
        {
            if (this.cbDataTableList.Text.Length == 0)
            {
                MessageBox.Show("请选择数据表！");
                return;
            }
            if (this.cbNameSpaceList.Text.Length == 0)
            {
                MessageBox.Show("请选择命名空间！");
                return;
            }
            if (this.txtEntityName.Text.Length == 0)
            {
                MessageBox.Show("请输入类的名称！");
                return;
            }

            var fbd = new FolderBrowserDialog { Description = "保存文件的目录" };
            if (_lastSelectFolder != null)
            {
                fbd.SelectedPath = _lastSelectFolder;
            }
            fbd.ShowDialog();

            string folder = fbd.SelectedPath;
            if (string.IsNullOrEmpty(folder))
                return;

            _lastSelectFolder = folder;

            if (!folder.EndsWith("\\"))
            {
                folder += "\\";
            }

            folder += "code\\";

            if (Directory.Exists(folder))
            {
                var result = MessageBox.Show("目录已存在，是否清空目录中的文件？", "", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    Directory.Delete(folder, true);
                    Directory.CreateDirectory(folder);
                }
            }

            CreateCodeOfEntity(folder);

            MessageBox.Show("生成成功！");
        }

        private void cbDataTableList_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.txtEntityName.Text = this.cbDataTableList.Text;
        }

        #endregion

        #region 生成代码

        //AutoGenEntity
        private void CreateCodeOfEntity(string folder)
        {
            var ns = this.cbNameSpaceList.Text;
            string fileOfCodeCreator = folder + ns + ".entity\\" + this.txtEntityName.Text + "Entity.cs";
            CreateDirectory(fileOfCodeCreator);

            StringBuilder codeBuffer = new StringBuilder(100000);
            codeBuffer.AppendLine("using System;");
            codeBuffer.AppendLine("using DoNet.Utility.Database.EntitySql.Attribute;");
            codeBuffer.AppendLine("using DoNet.Utility.Database.EntitySql.Entity;");
            codeBuffer.AppendLine("");
            codeBuffer.Append("namespace ").Append(ns).AppendLine(".Entity");
            codeBuffer.AppendLine("{"); //命名空间
            codeBuffer.AppendLine("    [Serializable]");
            codeBuffer.Append("    [Table(\"").Append(this.cbDataTableList.Text).AppendLine("\")]");
            codeBuffer.Append("    public class ").Append(this.txtEntityName.Text).AppendLine("Entity : BaseEntity");
            codeBuffer.AppendLine("    {"); //命名空间

            var columns = ColumnEnumerator.GetColumnsOfTable(this.cbDataTableList.Text);
            for (int i = 0; i < columns.Length; i++)
            {
                string objType = GetObjectTypeOfDBColumnForCode(columns[i].ColumnType);
                if (objType != "string")
                    objType += "?";

                string dbType = GetDBTypeFromRawType(columns[i].ColumnType);
                codeBuffer.AppendLine(string.Format("        [Field(\"{0}\")]", columns[i].ColumnName));
                codeBuffer.Append("        public ").Append(objType).Append(" ").Append(columns[i].ColumnName);
                codeBuffer.AppendLine(" { get; set; }");
            }

            codeBuffer.AppendLine("    }"); //Class
            codeBuffer.AppendLine("}"); //命名空间

            File.WriteAllText(fileOfCodeCreator, codeBuffer.ToString());
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 创建目标目录
        /// </summary>
        private void CreateDirectory(string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (dir != null && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        /// <summary>
        /// 根据数据库字段类型获取默认的数据类型(用于生成代码)
        /// </summary>
        /// <param name="dbType">数据库字段类型</param>
        /// <returns>数据类型</returns>
        private string GetObjectTypeOfDBColumnForCode(string dbType)
        {
            string retType = "string";
            if (string.IsNullOrEmpty(dbType))
                return retType;

            dbType = dbType.ToLower();

            if (dbType.Contains("char"))
                return retType;

            if (dbType.Contains("text"))
                return retType;

            switch (dbType)
            {
                case "bigint":
                    retType = "long";
                    break;
                case "bit":
                    retType = "bool";
                    break;
                case "date":
                    retType = "DateTime";
                    break;
                case "datetime":
                    retType = "DateTime";
                    break;
                case "datetime2":
                    retType = "DateTime";
                    break;
                case "decimal":
                    retType = "decimal";
                    break;
                case "float":
                    retType = "double";
                    break;
                case "int":
                    retType = "int";
                    break;
                case "money":
                    retType = "decimal";
                    break;
                case "numeric":
                    retType = "decimal";
                    break;
                case "smalldatetime":
                    retType = "DateTime";
                    break;
                case "smallint":
                    retType = "short";
                    break;
                case "smallmoney":
                    retType = "decimal";
                    break;
                case "tinyint":
                    retType = "byte";
                    break;
                default:
                    break;
            }

            return retType;
        }

        /// <summary>
        /// 根据数据库字段类型获取默认的DBType类型
        /// </summary>
        /// <param name="rawDbType">数据库字段类型</param>
        /// <returns>数据类型</returns>
        private string GetDBTypeFromRawType(string rawDbType)
        {
            string retType = "DbType.AnsiString";
            if (string.IsNullOrEmpty(rawDbType))
                return retType;
            rawDbType = rawDbType.ToLower();

            if (rawDbType.Contains("char"))
                return retType;

            if (rawDbType.Contains("text"))
                return retType;

            switch (rawDbType)
            {
                case "bigint":
                    retType = "DbType.Int64";
                    break;
                case "bit":
                    retType = "DbType.Boolean";
                    break;
                case "date":
                    retType = "DbType.DateTime";
                    break;
                case "datetime":
                    retType = "DbType.DateTime";
                    break;
                case "datetime2":
                    retType = "DbType.DateTime";
                    break;
                case "decimal":
                    retType = "DbType.Decimal";
                    break;
                case "float":
                    retType = "DbType.Double";
                    break;
                case "int":
                    retType = "DbType.Int32";
                    break;
                case "money":
                    retType = "DbType.Decimal";
                    break;
                case "numeric":
                    retType = "DbType.Decimal";
                    break;
                case "smalldatetime":
                    retType = "DbType.DateTime";
                    break;
                case "smallint":
                    retType = "DbType.Int16";
                    break;
                case "smallmoney":
                    retType = "DbType.Decimal";
                    break;
                case "tinyint":
                    retType = "DbType.Byte";
                    break;
                default:
                    break;
            }

            return retType;
        }

        /// <summary>
        /// 显示命名空间列表
        /// </summary>
        private void ShowNameSpaceList()
        {
            this.cbNameSpaceList.Items.Clear();
            var nameSpaces = MyConfiguations.NameSpaceList;
            for (int i = 0; i < nameSpaces.Count; i++)
            {
                string loopKey = nameSpaces.Keys[i];
                string loopNameSpace = nameSpaces[loopKey];
                this.cbNameSpaceList.Items.Add(loopNameSpace);
            }
            if (nameSpaces.Count > 0)
            {
                this.cbNameSpaceList.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// 显示数据表
        /// </summary>
        private void ShowDataTables()
        {
            string[] tables = TableEnumerator.GetUserTables();
            tables = tables.OrderBy(n => n).ToArray();
            this.cbDataTableList.Items.Clear();
            this.cbDataTableList.Items.AddRange(tables);
        }

        private string FirstCharLower(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                str = str.Replace("_", "");
                var first = str.Substring(0, 1);
                var last = str.Substring(1, str.Length - 1);
                return first.ToLower() + last;
            }
            return string.Empty;
        }

        #endregion
    }
}