using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PetaPoco
{
    public static class PetaPocoExtensions
    {
        private static readonly Regex rxSelect = new Regex(@"\A\s*(SELECT|EXECUTE|CALL)\s", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Multiline);
        private static readonly Regex rxFrom = new Regex(@"\A\s*FROM\s", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        public static bool AnyExist<T>(this Database db, string sql, params object[] args)
        {
            Page<T> results = db.Page<T>(1, 1, sql, args);
            return (results.TotalItems > 0);
        }

        public static List<T> FetchAll<T>(this Database db)
        {
            return db.Query<T>(string.Empty, null).ToList();
        }

        public static List<T1> FetchJoined<T1, T2>(this Database db, string T1_foreign_key_name, string sql, params object[] args)
        {
            if (db.EnableAutoSelect)
                sql = AddSelectClause<T1, T2>(db, T1_foreign_key_name, sql);

            return db.Query<T1, T2>(sql, args).ToList();
        }

        public static List<T1> FetchAllJoined<T1, T2>(this Database db)
        {
            return FetchJoined<T1, T2>(db, string.Empty, null);
        }

        public static T1 SingleOrDefaultJoined<T1, T2>(this Database db, string T1_foreign_key_name, object primaryKey)
        {
            string sql = string.Format("WHERE {0}=@0", db.EscapeSqlIdentifier(PetaPoco.Database.PocoData.ForType(typeof(T1)).TableInfo.PrimaryKey));
            
            if (db.EnableAutoSelect)
                sql = AddSelectClause<T1, T2>(db, T1_foreign_key_name, sql);

            return db.Query<T1, T2>(sql, primaryKey).SingleOrDefault();
        }

        private static string AddSelectClause<T1, T2>(Database db, string T1_foreign_key_name, string sql)
        {
            if (sql.StartsWith(";"))
                return sql.Substring(1);

            if (!rxSelect.IsMatch(sql))
            {
                var pd1 = PetaPoco.Database.PocoData.ForType(typeof(T1));
                var pd2 = PetaPoco.Database.PocoData.ForType(typeof(T2));

                var tableName1 = db.EscapeTableName(pd1.TableInfo.TableName);
                var tableName2 = db.EscapeTableName(pd2.TableInfo.TableName);

                string cols1 = string.Join(", ", (from c in pd1.QueryColumns
                                                  select tableName1 + "." + db.EscapeSqlIdentifier(c)).ToArray());

                string cols2 = string.Join(", ", (from c in pd2.QueryColumns
                                                  select tableName2 + "." + db.EscapeSqlIdentifier(c)).ToArray());
                if (!rxFrom.IsMatch(sql))
                    sql = string.Format("SELECT {0}, {1} FROM {2} INNER JOIN {3} ON {2}.{4} = {3}.{5} {6}",
                        cols1, cols2, tableName1, tableName2, T1_foreign_key_name, pd2.TableInfo.PrimaryKey, sql);
                else
                    sql = string.Format("SELECT {0}, {1} {2}", cols1, cols2, sql);
            }
            return sql;
        }
    }
}
