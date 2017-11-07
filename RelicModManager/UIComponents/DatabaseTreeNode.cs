using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RelhaxModpack
{
    class DatabaseTreeNode : TreeNode
    {
        public Dependency GlobalDependency { get; set; }
        public Dependency Dependency { get; set; }
        public LogicalDependnecy LogicalDependency { get; set; }
        public SelectableDatabasePackage DatabaseObject { get; set; }
        public Category Category { get; set; }
        public DatabaseTreeNode(Object o, int mode)
        {
            //0 = global dependency
            //1 = dependency
            //2 = logical dependency
            //3 = DBO
            //4 = category
            switch (mode)
            {
                case 0:
                    GlobalDependency = (Dependency)o;
                    Dependency = null;
                    LogicalDependency = null;
                    DatabaseObject = null;
                    Category = null;
                    this.Text = GlobalDependency.packageName;
                    break;
                case 1:
                    GlobalDependency = null;
                    Dependency = (Dependency)o;
                    LogicalDependency = null;
                    DatabaseObject = null;
                    Category = null;
                    this.Text = Dependency.packageName;
                    break;
                case 2:
                    GlobalDependency = null;
                    Dependency = null;
                    LogicalDependency = (LogicalDependnecy)o;
                    DatabaseObject = null;
                    Category = null;
                    this.Text = LogicalDependency.packageName;
                    break;
                case 3:
                    GlobalDependency = null;
                    Dependency = null;
                    LogicalDependency = null;
                    DatabaseObject = (SelectableDatabasePackage)o;
                    Category = null;
                    this.Text = DatabaseObject.packageName;
                    break;
                case 4:
                    GlobalDependency = null;
                    Dependency = null;
                    LogicalDependency = null;
                    DatabaseObject = null;
                    Category = (Category)o;
                    this.Text = Category.name;
                    break;
            }

        }
    }
}
