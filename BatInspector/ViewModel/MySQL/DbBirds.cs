

using System.Collections.Generic;
using System.Security.Permissions;
using System.Windows.Documents;

namespace BatInspector
{
  public class DbBirds
  {
    DataBase _db;
    bool _isOpen = false;
    List <sqlRow> __query;

    public bool IsOpen { get { return _isOpen; } }

    public List<sqlRow> QueryResult { get { return __query; }  set { __query = value; } }
    
    public void setIsOpen(bool isOpen) { _isOpen = isOpen; }

    public DbBirds(DataBase db)
    {
    _db = db;
    }
  }
}
