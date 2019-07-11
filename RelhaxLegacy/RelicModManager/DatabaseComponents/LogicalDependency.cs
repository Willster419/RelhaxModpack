using RelhaxModpack.DatabaseComponents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RelhaxModpack
{
    public class LogicalDependency : DatabasePackage
    {
        //list of linked mods and configs that use 
        public List<DatabaseLogic> DatabasePackageLogic = new List<DatabaseLogic>();
        //acts as a NOT flag
        public bool NegateFlag;
        //handle linked dependencies as AND or OR logic
        public AndOrFlag AndOrLogic = AndOrFlag.AND;
        public LogicalDependency() {}
        //for the tostring thing
        public override string ToString()
        {
            return NegateFlag ? "(Not) " + PackageName : "" + PackageName;
        }

        public enum AndOrFlag
        {
            None,
            AND,
            OR
        }

        public static string GetAndOrString(AndOrFlag flag)
        {
            if (flag == AndOrFlag.AND)
                return "AND";
            else
                return "OR";
        }

        public static AndOrFlag GetAndOrID(string s)
        {
            foreach (AndOrFlag f in Enum.GetValues(typeof(AndOrFlag)))
            {
                if (GetAndOrString(f).ToLower().Equals(s.ToLower()))
                    return f;
            }
            Logging.Manager("Get invalid AndOrString. Reporting default AND flag");
            return AndOrFlag.AND;
        }
    }
}
