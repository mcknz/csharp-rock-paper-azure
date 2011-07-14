using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using Compete.Site.Infrastructure;
using Compete.Site.Models;
using RockPaperScissorsPro;

namespace Compete.Bot.Validation
{
    [Serializable]
    public class BotValidationError
    {
        public String error { get; private set; }
        public String code { get; private set; }

        public BotValidationError(string _error, string _code = null) 
        { 
            error = _error; 
            code = _code; 
        }

        public String HtmlEncode()
        {
            String errorPart = String.Empty;
            String codePart = String.Empty;      
            HttpContext context = HttpContext.Current;

            errorPart = this.error.Replace('\\', '/');
            if (context != null) errorPart = context.Server.HtmlEncode(this.error);

            if (!String.IsNullOrEmpty(this.code))
            {
                if (context == null)
                    codePart = this.code;
                else
                {
                    codePart = this.code.Replace(" ", ";nbsp;").Replace("\t", ";nbsp;;nbsp;;nbsp;;nbsp;");
                    codePart = context.Server.HtmlEncode(codePart);
                    codePart = codePart.Replace(";nbsp;", "&nbsp;");
                }
                codePart = codePart.Replace(System.Environment.NewLine, "<br/>");
                return String.Format("{0}<br/><code>{1}</code><br/><br/>", errorPart, codePart);
            }
            else
            {
                return errorPart;
            }
        }
    }

    public static class BotValidator
    {
        public static String ValidateBot(HttpPostedFileBase hpf, out List<BotValidationError> errList)
        {
            errList = new List<BotValidationError>();

            Boolean invalidExtension = false;
            String fileExtension = Path.GetExtension(hpf.FileName);
            invalidExtension = String.IsNullOrEmpty(fileExtension);

            String dllPath = String.Empty;
            if (!invalidExtension)
            {
                switch (fileExtension.ToLower())
                {
                    case ".dll":
                        dllPath = ValidateAssembly(hpf, ref errList);
                        break;
                    case ".py":
                        dllPath = ValidateScript(BotCompiler.Language.Python, hpf, ref errList);
                        break;
                    case ".rb":
                        dllPath = ValidateScript(BotCompiler.Language.Ruby, hpf, ref errList);
                        break;
                    default:
                        invalidExtension = true;
                        break;
                }
            }
            if (invalidExtension)
                errList.Add(new BotValidationError("Only files with extensions of .dll, .py, and .rb are supported."));

            return dllPath;
        }

        // use reflection in ad hoc AppDomain to verify required interfaces are implemented 
        private static IEnumerable<BotValidationError> ValidateDotNetInterfaces(String dllPath)
        {
            var errList = new List<BotValidationError>();
            try
            {
                DynamicAssemblyTypeFinder dynamicAssemblyTypeFinder = new DynamicAssemblyTypeFinder();
                dynamicAssemblyTypeFinder.AddAll(new AssemblyFile[] { new AssemblyFile(dllPath) }.ToList<AssemblyFile>());

                // check that IBotFactory is implemented exactly once
                var botFactoryList = dynamicAssemblyTypeFinder.Create<Compete.Bot.IBotFactory>();
                switch (botFactoryList.Count())
                {
                    case 0: errList.Add(new BotValidationError("IBotFactory interface not implemented"));
                        break;
                    case 1:
                        break;
                    default: errList.Add(new BotValidationError("IBotFactory interface implemented more than once"));
                        break;
                }

                // check that IRockPaperScissorsBot is implemented exactly once
                var botList = dynamicAssemblyTypeFinder.Create<RockPaperScissorsPro.IRockPaperScissorsBot>();
                switch (botList.Count())
                {
                    case 0: errList.Add(new BotValidationError("IRockPaperScissorsBot interface not implemented"));
                        break;
                    case 1:
                        break;
                    default: errList.Add(new BotValidationError("IRockPaperScissorsBot interface implemented more than once"));
                        break;
                }
            }
            catch (Exception e)
            {
                errList.Add(new BotValidationError("ValidateDotNetInterfaces: " + e.Message));
            }

            return errList;
        }

        // grab uploaded DLL (C#, VB, F#) and place it in staging area to validate that DLL implements required interfaces
        // ValidateDotNetInterfaces is invoked in a separate AppDomain to make the determination
        private static String ValidateAssembly(HttpPostedFileBase file, ref List<BotValidationError> errList)
        {
            var dllPath = String.Empty;
            try
            {
                // save uploaded file in temp area
                dllPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(file.FileName));
                file.SaveAs(dllPath);

                // validate implementation of the required interfaces
                using (var staging = new StagingArea())
                {
                    var reflectionErrors = AppDomainHelper.InSeparateAppDomain<String, IEnumerable<BotValidationError>>(staging.Root, dllPath, ValidateDotNetInterfaces);
                    foreach (var err in reflectionErrors)
                        errList.Add(err);
                }
            }
            catch (Exception e)
            {
                errList.Add(new BotValidationError("ValidateDll: " + e.Message));
                dllPath = String.Empty;
            }

            // return assembly path (or empty if there's a problem)
            return dllPath;
        }

        // run dynamic language implementation in DLR to verify implementation (as much as possible)
        private static IEnumerable<BotValidationError> ValidateViaDlr(String dllPath)
        {
            var errList = new List<BotValidationError>();
            var theScript = String.Empty;
            try
            {
                DynamicAssemblyTypeFinder dynamicAssemblyTypeFinder = new DynamicAssemblyTypeFinder();
                dynamicAssemblyTypeFinder.AddAll(new AssemblyFile[] { new AssemblyFile(dllPath) }.ToList<AssemblyFile>());

                // use reflection to get the script (used later for error messaging)
                theScript = (String) dynamicAssemblyTypeFinder.FindType("RockPaperAzure.MyBot").GetField("theScript").GetValue(null);

                // instantiate the bot
                dynamic theBot = dynamicAssemblyTypeFinder.CreateOne<RockPaperScissorsPro.IRockPaperScissorsBot>();

                // run the MakeMove method (just to see if it's there)
                theBot.MakeMove(new Player("1", null), new Player("2", null), GameRules.Default);

            }
            catch (Exception e)
            {
                // many exceptions will be nested in a TargetInvocationException
                Exception exception = e.InnerException;
                if (exception != null)
                {
                    if (exception.GetType() == typeof(Microsoft.Scripting.SyntaxErrorException))
                    {
                        var se = (Microsoft.Scripting.SyntaxErrorException) exception;

                        String[] lines = theScript.Split(new String[] { System.Environment.NewLine }, StringSplitOptions.None);
                        String codeLine = ((lines.Length >= se.Line) && (se.Line > 0)) ? lines[se.Line - 1] : null;
                        String markerLine = ((codeLine != null) && (se.RawSpan.Start.Column > 0)) ? 
                            System.Environment.NewLine + new String(' ', se.RawSpan.Start.Column - 1) + "^" : String.Empty;

                        errList.Add(new BotValidationError(
                            String.Format("Line {0}: {1}", se.Line, se.Message), 
                            String.Format("{0}{1}", codeLine, markerLine)));
                    }
                    else
                    {
                        errList.Add(new BotValidationError(exception.Message));
                    }
                }
                else
                {
                    errList.Add(new BotValidationError(e.Message));
                }
            }

            return errList;
        }

        // get uploaded script (Python or Ruby), compile it, and then place DLL in staging area for error checking
        // ValidateViaDlr is invoked in a separate AppDomain to run the bot and so detect (some) syntax errors
        private static String ValidateScript(BotCompiler.Language lang, HttpPostedFileBase file, ref List<BotValidationError> errList)
        {
            var dllPath = String.Empty;
            try
            {
                // save uploaded file in temp area
                String tmpFile = Path.Combine(Path.GetTempPath(), Path.GetFileName(file.FileName));
                file.SaveAs(tmpFile);

                // compile file into assembly
                dllPath = BotCompiler.Compile(lang, tmpFile, ref errList);

                // validate dynamic language script
                using (var staging = new StagingArea())
                {
                    var reflectionErrors = AppDomainHelper.InSeparateAppDomain<String, IEnumerable<BotValidationError>>(staging.Root, dllPath, ValidateViaDlr);
                    foreach (var err in reflectionErrors)
                        errList.Add(err);
                }
            }
            catch (Exception e)
            {
                errList.Add(new BotValidationError("ValidateScript: " + e.Message));
                dllPath = String.Empty;
            }

            return dllPath;
        }

        public static bool ValidateSignature(string textToValidate, string signature)
        {
            const string publicKey = @"<RSAKeyValue><Modulus>qwEK9Idbsbeu0kTo76ypPQXJQj2ABODnZ1i/6DapDzsYCWb0lEVs0043QyjeSsW2R1StZeU5QzwkBRyqKPQcEIzKmaSVZWZbDn/dn1J7bEqUeLH3fuh7V3q6iGX5JKzD57nB/qmHBoaK9wMYMsKNBejGdxUYM61G24K5LevcT5E=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

            // get the bytes of the signature itself
            byte[] signatureBytes = System.Web.HttpServerUtility.UrlTokenDecode(signature);

            // get the bytes of the text to verify
            byte[] plainBytes = System.Text.Encoding.ASCII.GetBytes(textToValidate);
            RSACryptoServiceProvider rsacheck = new RSACryptoServiceProvider();

            // setting public key to checking RSA class
            rsacheck.FromXmlString(publicKey);

            // create hash instance
            HashAlgorithm hashAlg = HashAlgorithm.Create("SHA1");

            // deformatter is used to check signed data
            AsymmetricSignatureDeformatter signcheck = new RSAPKCS1SignatureDeformatter(rsacheck);
            signcheck.SetHashAlgorithm("SHA1");

            // check signature
            return signcheck.VerifySignature(hashAlg.ComputeHash(plainBytes), signatureBytes);
        }
    }
}