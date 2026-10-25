/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using BatInspectorPublisher.Adapters.INaturalist;
using BatInspectorPublisher.Core;
using BatInspectorPublisher.Core.InputSchema;
using BatInspectorPublisher.Core.Results;
using libParser;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace BatInspector
{
  public class ExportPlatforms
  {

    public static string getKey()
    {
      [DllImport("libTools.dll", CallingConvention = CallingConvention.Cdecl)]
      static extern IntPtr retrieveKey();
      [DllImport("libTools.dll", CallingConvention = CallingConvention.Cdecl)]
      static extern void freeString(IntPtr str);

      IntPtr ptr = retrieveKey();
      string retVal = Marshal.PtrToStringAnsi(ptr);
      freeString(ptr);
      return retVal;

    }


    /// <summary>
    /// generate ke file from file that contains unencrypted secrets
    /// </summary>
    /// <param name="fileName"></param>
    public static void genKeyFile(string fileName)
    {
      string[] lines = File.ReadAllLines(fileName);
      if(lines.Length != 2)
      {
        DebugLog.log("secret file should contain exactly 2 lines", enLogType.ERROR);
        return;
      }
      string[] token = lines[0].Split('=');
      if(token.Length != 2)
      {
        DebugLog.log($"secret file line 1: format error: {lines[0]}", enLogType.ERROR);
        return;
      }
      string a = token[1].Replace("\"","");
      token = lines[1].Split('=');
      if (token.Length != 2)
      {
        DebugLog.log($"secret file line 2: format error: {lines[1]}", enLogType.ERROR);
        return;
      }
      string b = token[1].Replace("\"", "");
      string key = getKey();
      string encrA = Encrypt(a, key);
      string encrB = Encrypt(b, key);
      string file = encrA + "\n" + encrB + "\n";
      File.WriteAllText(AppParams.API_KEYS_CRYPT, file);
    }


    public async Task<RunReport?> upload(string expName, bool commit = false)
    {
      try
      {
        if(commit)
          DebugLog.log("starting export to iNaturalist, this may take a while...", enLogType.INFO);
        else
          DebugLog.log("starting data check on iNaturalist, this may take a while...", enLogType.INFO);
        string[] lines = File.ReadAllLines(AppParams.API_KEYS_CRYPT);
        string key = getKey();
        string clientId = ExportPlatforms.Decrypt(lines[0], key);
        string clientSecret = ExportPlatforms.Decrypt(lines[1], key);

        INaturalistOptions options = new INaturalistOptions { ClientId = clientId, ClientSecret = clientSecret };

        using HttpClient http = new HttpClient();
        INaturalistAuthenticator auth = new INaturalistAuthenticator(options, http);           // Windows: DPAPI-encrypted token store
        INaturalistPublisher publisher = new INaturalistPublisher(options, http, auth);

        // login with user account to iNaturalist
        if (!auth.HasStoredToken)
          await auth.LoginAsync();

        InputDocument input = InputSchemaReader.ReadFile(expName);
        ExportOrchestrator orchestrator = new ExportOrchestrator(publisher);

        RunReport preview = await orchestrator.RunAsync(input, new PublishOptions { Commit = commit });
        return preview;
      }
      catch (Exception ex)
      {
        DebugLog.log($"{ex}", enLogType.ERROR);
        return null;
      }
    }

    /// <summary>
    /// Verschlüsselt einen Text mit AES.
    /// </summary>
    public static string Encrypt(string plainText, string key)
    {
      using (Aes aes = Aes.Create())
      {
        aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32, '0').Substring(0, 32));
        aes.GenerateIV();

        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        using (MemoryStream ms = new MemoryStream())
        {
          // IV vor dem Chiffretext speichern
          ms.Write(aes.IV, 0, aes.IV.Length);

          using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
          using (StreamWriter sw = new StreamWriter(cs))
          {
            sw.Write(plainText);
          }

          return Convert.ToBase64String(ms.ToArray());
        }
      }
    }

    /// <summary>
    /// Entschlüsselt einen Text mit AES.
    /// </summary>
    public static string Decrypt(string cipherText, string key)
    {
      byte[] fullCipher = Convert.FromBase64String(cipherText);
      byte[] iv = new byte[16];
      byte[] cipher = new byte[fullCipher.Length - iv.Length];

      Array.Copy(fullCipher, iv, iv.Length);
      Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);

      using (Aes aes = Aes.Create())
      {
        aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32, '0').Substring(0, 32));
        aes.IV = iv;

        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using (MemoryStream ms = new MemoryStream(cipher))
        using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
        using (StreamReader sr = new StreamReader(cs))
        {
          return sr.ReadToEnd();
        }
      }
    }
  }
}
