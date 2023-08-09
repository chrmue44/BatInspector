/********************************************************************************
 *               Author: Christian Müller
 *      Date of cration: 2021-08-10                                       
 *   Copyright (C) 2022: christian Müller christian(at)chrmue(dot).de
 *
 *              Licence:
 * 
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
 * EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 ********************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace libParser
{
  public class Utils
  {
    public static List<string> getInpList(string inpFile)
    {
      List<string> retVal = new List<string>();
      try
      {
        StreamReader file = new StreamReader(inpFile);
        do
        {
          string line = file.ReadLine();
          if (line == null)
            break;
          line = line.Replace("\"", "");
          if(line.Length > 1)
            retVal.Add(line);
        } while (true);
        file.Close();
      }
      catch
      {
//        DebugLog.log("error opening file: " + inpFile);
      }
      return retVal;
    }

    public static bool isAlphaNum(char c)
    {
      string specChars = "&/%\\-*._";
      return ((c >= 'A') && (c <= 'Z')) ||
             ((c >= 'a') && (c <= 'z')) ||
             ((c >= '0') && (c <= '9')) ||
             (specChars.IndexOf(c) >= 0)
             ;
    }

    public static bool isNum(string val, bool withDecimalPoint = false)
    {
      bool atLeastOneDigit = false;
      foreach(char c in val)
      {
        atLeastOneDigit |= ((c >= '0') && (c <= '9'));
        if (!isNum(c, withDecimalPoint))
          return false;
      }
      return atLeastOneDigit;
    }

    public static bool isNum(char c, bool withDecimalPoint = false)
    {
     bool retVal = ((c >= '0') && (c <= '9')) || (c=='-') || (c=='+') ;
      if (withDecimalPoint)
        retVal  = retVal || (c == '.') || (c == 'e') || (c == 'E');
      return retVal;
    }

    // prueft, ob der uebergebene Character ein erlaubter HEX-Character ist
    public static bool ishex(char Ch)
    {
      if (
    ((Ch >= '0') && (Ch <= '9')) ||
    ((Ch >= 'A') && (Ch <= 'F')) ||
    ((Ch >= 'a') && (Ch <= 'f'))
   )
        return true;
      else
        return false;
    }
    public static bool isWhiteSpace(char c)
    {
      return (c == ' ') || (c == '\t');
    }

    static public void CopyFolder(string sourceFolder, string destFolder)
    {
      //http://www.csharp411.com/c-copy-folder-recursively/
      if (!Directory.Exists(destFolder))
        Directory.CreateDirectory(destFolder);
      string[] files = Directory.GetFiles(sourceFolder);
      copyFiles(files, destFolder);
      string[] folders = Directory.GetDirectories(sourceFolder);
      foreach (string folder in folders)
      {
        string name = Path.GetFileName(folder);
        string dest = Path.Combine(destFolder, name);
        CopyFolder(folder, dest);
      }
    }

    static public void copyFiles(string[] files, string dstFolder)
    {
      foreach (string file in files)
      {
        string name = Path.GetFileName(file);
        string dest = Path.Combine(dstFolder, name);
        File.Copy(file, dest);
      }
    }

    static public string CreateTempDirectory()
    {
      string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
      Directory.CreateDirectory(tempDirectory);
      return tempDirectory;
    }

    public static bool isNetworkPath(string path)
    {
      if (!path.StartsWith(@"/") && !path.StartsWith(@"\"))
      {
        string rootPath = System.IO.Path.GetPathRoot(path); // get drive's letter
        System.IO.DriveInfo driveInfo = new System.IO.DriveInfo(rootPath); // get info about the drive
        return driveInfo.DriveType == DriveType.Network; // return true if a network drive
      }

      return true; // is a UNC path
    }

    /// <summary>
    /// remove all non numeric characters from string
    /// </summary>
    /// <param name="strIn"></param>
    /// <returns></returns>
    public static string removeNonNumerics(string strIn, bool withDecimalPoint = false)
    {
      string retVal = "";
      for(int i = 0; i < strIn.Length; i++)
      {
        char c = strIn[i];
        if (isNum(c, withDecimalPoint))
          retVal += c;
      }
      return retVal;
    }

    /// <summary>
    /// replace all numeric characters in string
    /// </summary>
    /// <param name="strIn"></param>
    /// <returns></returns>
    public static string replaceNumerics(string strIn, char replacement)
    {
      string retVal = "";
      for (int i = 0; i < strIn.Length; i++)
      {
        char c = strIn[i];
        if (isNum(c, false))
          retVal += replacement;
        else
          retVal += c;
      }
      return retVal;
    }

    /// <summary>
    /// replace all calculation terms in a string
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    /// 
    public static int replaceCalculations(ref string cell, out string err, MethodList mList = null, VarList varlist = null)
    {
      int pos;
      int retVal = 0;
      err = "";
      do
      {
        pos = cell.IndexOf("=(");
        if (pos >= 0)
        {
          string newCell = cell.Substring(0, pos);
          int pos2 = findMatchingClosingBrace(cell, pos + 2);
          if (pos2 >= 0)
          {
            string formula = cell.Substring(pos + 2, pos2 - pos - 2);
            AnyType val;
            Expression exp = new Expression(varlist);
            if (mList != null)
              exp.addMethodList(mList);

            val = exp.parse(formula);
            if(exp.Errors > 0)
            {
              err = "Error parsing formula: " + formula;
              retVal = 3;
              break;
            }
            newCell += val.ToString();
            newCell += cell.Substring(pos2 + 1);
            cell = newCell;
          }
          else
          {
            err = "found '=(' in cell but closing ')' is missing!: " + cell;
            retVal = 2;
            break;
          }
        }
      } while (pos >= 0);
      return retVal;
    }
    /// <summary>
    /// find the matching closing brace . The search begins after opening brace
    /// </summary>
    /// <param name="str">string to search</param>
    /// <param name="startPos"></param>
    /// <returns>position of the matching closing brace, or -1 if not found</returns>
    public static int findMatchingClosingBrace(string str, int startPos)
    {
      char open = '(';
      char close = ')';
      int count = 1;
      int retVal = -1;
      for (int i = startPos; i < str.Length; i++)
      {
        if (str[i] == open)
          count++;
        else if (str[i] == close)
          count--;
        if (count == 0)
        {
          retVal = i;
          break;
        }
      }
      return retVal;
    }

    public static string makeUnicosName(string name)
    {
      string retVal = null;
      int pos = name.IndexOf('+');
      if(pos >= 0)
      {
        int pos2 = name.IndexOf('-');
        if(pos2 > pos)  
          retVal = name.Substring(0, pos) + name.Substring(pos2);
      }
      if (retVal == null)
        retVal = name;
      retVal = retVal.Replace("=", "");
      retVal = retVal.Replace(".", "");
      retVal = retVal.Replace("+", "");
      retVal = retVal.Replace("-", "");
      return retVal;
    }

    public static string LTrim(string str)
    {
      string retVal = "";
      int pos = 0;
      foreach(char ch in str)
      {
        if (!isspace(ch))
          break;
        pos++;
      }
      if(pos < str.Length)
        retVal = str.Substring(pos);
      return retVal;
    }

    public static bool isalpha(char ch)
    {
      return System.Char.IsLetter(ch);
    }

    public static bool isspace(char ch)
    {
      return (ch == ' ') || (ch == '\t');
    }

    public static bool isdigit(char ch)
    {
      return System.Char.IsNumber(ch);
    }

    public static bool isalnum(char ch)
    {
      return isalpha(ch) || isdigit(ch);
    }

    /// <summary>
    // The main function that checks if  
    // two given strings match. The first string  
    // may contain wildcard characters 
    // https://www.geeksforgeeks.org/wildcard-character-matching/
    /// </summary>
    /// <param name="pattern"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool matchPattern(string pattern, string name)
    {

      // If we reach at the end of both strings,  
      // we are done 
      if (pattern.Length == 0 && name.Length == 0)
        return true;

      // Make sure that the characters after '*'  
      // are present in second string.  
      // This function assumes that the first 
      // string will not contain two consecutive '*' 
      if (pattern.Length > 1 && pattern[0] == '*' &&
                                name.Length == 0)
        return false;

      // If the first string contains '?',  
      // or current characters of both strings match 
      if ((pattern.Length > 1 && pattern[0] == '?') ||
          (pattern.Length != 0 && name.Length != 0 &&
           pattern[0] == name[0]))
        return matchPattern(pattern.Substring(1),
                     name.Substring(1));

      // If there is *, then there are two possibilities 
      // a) We consider current character of second string 
      // b) We ignore current character of second string. 
      if (pattern.Length > 0 && pattern[0] == '*')
        return matchPattern(pattern.Substring(1), name) ||
               matchPattern(pattern, name.Substring(1));
      return false;
    }

    /// <summary>
    /// create a relative path from two absolute paths
    /// https://stackoverflow.com/questions/9042861/how-to-make-an-absolute-path-relative-to-a-particular-folder
    /// </summary>
    /// <param name="relativeTo">root point from where the path should be relative</param>
    /// <param name="">path</param>
    /// <returns>a relative path</returns>
    public static string relativePath(string relativeTo, string path)
    {
      path = Path.GetFullPath(path);
      relativeTo = Path.GetFullPath(relativeTo);

      var separators = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
      IReadOnlyList<string> p1 = path.Split(separators);
      IReadOnlyList<string> p2 = relativeTo.Split(separators, StringSplitOptions.RemoveEmptyEntries);

      StringComparison sc = new StringComparison();

      int i;
      int n = Math.Min(p1.Count, p2.Count);
      for (i = 0; i < n; i++)
        if (!string.Equals(p1[i], p2[i], sc))
          break;

      if (i == 0)
      {
        // Cannot make a relative path, for example if the path resides on another drive.
        return path;
      }

      p1 = p1.Skip(i).Take(p1.Count - i).ToList();

      if (p1.Count == 1 && p1[0].Length == 0)
        p1 = Array.Empty<string>();

      string relativePath = string.Join(
          new string(Path.DirectorySeparatorChar, 1),
          Enumerable.Repeat("..", p2.Count - i).Concat(p1));

      if (relativePath.Length == 0)
        relativePath = ".";

      return relativePath;
    }

    public static string latToString(double lat)
    {
      string hem = " N";
      if (lat < 0)
      {
        lat *= -1;
        hem = " S";
      }
      int deg = (int)lat;
      double min = (lat - deg) * 60;
      return deg.ToString() + "° " + min.ToString("0.000") + hem;
    }

    public static string lonToString(double lon)
    {
      string hem = " E";
      if (lon < 0)
      {
        lon *= -1;
        hem = " W";
      }
      int deg = (int)lon;
      double min = (lon - deg) * 60;
      return deg.ToString() + "° " + min.ToString("0.000") + hem;
    }
  }
}
