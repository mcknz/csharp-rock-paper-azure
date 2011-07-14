using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Microsoft.CSharp;

namespace Compete.Bot.Validation
{
    public static class BotCompiler
    {
        public enum Language { Python, Ruby };

        public static String Compile(Language language, String scriptFile, ref List<BotValidationError> errList)
        {
            String outputAssembly = default(String);
            String partialClassFile = default(String);
            try
            {
                String binPath = HttpContext.Current.Server.MapPath("~/bin");
                String srcPath = Path.Combine(RockPaperAzure.AzureHelper.GetLocalFolder(), "src");

                CSharpCodeProvider provider = new CSharpCodeProvider();

                CompilerParameters cp = new CompilerParameters();
                cp.GenerateExecutable = false;
                cp.ReferencedAssemblies.Add("System.dll");
                cp.ReferencedAssemblies.Add("System.Core.dll");
                cp.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
                cp.ReferencedAssemblies.Add(Path.Combine(binPath, "Microsoft.Dynamic.dll"));
                cp.ReferencedAssemblies.Add(Path.Combine(binPath, "Microsoft.Scripting.dll"));
                cp.ReferencedAssemblies.Add(Path.Combine(binPath, "Microsoft.Scripting.Metadata.dll"));
                cp.ReferencedAssemblies.Add(Path.Combine(binPath, "Compete.Bot.dll"));
                cp.ReferencedAssemblies.Add(Path.Combine(binPath, "RockPaperScissorsPro.RPA.dll"));

                if (language == Language.Python)
                {
                    cp.ReferencedAssemblies.Add(Path.Combine(binPath, "IronPython.dll"));
                    cp.ReferencedAssemblies.Add(Path.Combine(binPath, "IronPython.Modules.dll"));
                }
                if (language == Language.Ruby)
                {
                    cp.ReferencedAssemblies.Add(Path.Combine(binPath, "IronRuby.dll"));
                    cp.ReferencedAssemblies.Add(Path.Combine(binPath, "IronRuby.Libraries.dll"));
                    cp.ReferencedAssemblies.Add(Path.Combine(binPath, "IronRuby.Libraries.Yaml.dll"));
                }

                cp.OutputAssembly = outputAssembly = GetTempAssemblyFileName();

                List<String> fileList = new List<String>();
                switch (language)
                {
                    case Language.Python:
                        fileList.Add(Path.Combine(srcPath, "MyBotPython.cs"));
                        fileList.Add(Path.Combine(srcPath, "MyBotFactory.cs"));

                        partialClassFile = GeneratePartialClass(scriptFile, ref errList);
                        if (!String.IsNullOrEmpty(partialClassFile)) fileList.Add(partialClassFile);
                        break;

                    case Language.Ruby:
                        fileList.Add(Path.Combine(srcPath, "MyBotRuby.cs"));
                        fileList.Add(Path.Combine(srcPath, "MyBotFactory.cs"));

                        partialClassFile = GeneratePartialClass(scriptFile, ref errList);
                        if (!String.IsNullOrEmpty(partialClassFile)) fileList.Add(partialClassFile);
                        break;
                }

                if (errList.Count == 0)
                {
                    // compile the code
                    CompilerResults cr = provider.CompileAssemblyFromFile(cp, fileList.ToArray<String>());
                    if (cr.Errors.HasErrors)
                        errList.AddRange(from CompilerError e in cr.Errors
                                         select new BotValidationError(String.Format("{0}:{1} {2}", Path.GetFileName(e.FileName), e.Line, e.ErrorText)));
                }
            }
            catch (Exception e)
            {
                errList.Add(new BotValidationError("Compile: " + e.Message));
                if (!String.IsNullOrEmpty(outputAssembly)) File.Delete(outputAssembly);
            }
            finally
            {
                // delete the partial class file to clean up
                try
                {
                    File.Delete(partialClassFile);
                }
                catch (Exception)
                {
                }
            }

            return (errList.Count == 0) ? outputAssembly : null;
        }

        private static String GeneratePartialClass(String scriptFile, ref List<BotValidationError> errList)
        {
             String tmpFile = String.Empty;

            try
            {
                String scriptText = String.Empty;
                using (TextReader tr = new StreamReader(scriptFile))
                {
                    scriptText = tr.ReadToEnd();
                }

                tmpFile = Path.GetTempFileName();
                using (TextWriter tw = new StreamWriter(tmpFile))
                {
                    tw.Write(@"
#pragma warning disable 414
namespace RockPaperAzure
{
    public partial class MyBot
    {
        public static string theScript = @""" + scriptText.Replace("\"", "\"\"") + "\";" +
@"
    }
}");
                }
            }
            catch (Exception e)
            {
                errList.Add(new BotValidationError("GeneratePartialClass: " + e.Message));
                if (!String.IsNullOrEmpty(tmpFile))
                {
                    File.Delete(tmpFile);
                    tmpFile = String.Empty;
                }
            }
            return tmpFile;
        }

        private static String GetTempAssemblyFileName()
        {
            String dllFile = String.Empty;

            try
            {
                String tmpFile = Path.GetTempFileName();
                dllFile = Path.ChangeExtension(tmpFile, "dll");

                if (File.Exists(dllFile)) File.Delete(dllFile); 
                File.Move(tmpFile, dllFile);
            }
            catch (Exception)
            {
                // exceptions are unlikely, but possible unless file system transactions are used
                // see http://msdn.microsoft.com/en-us/magazine/cc163388.aspx
            }

            return dllFile;
        }
    }
}
