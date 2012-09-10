using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;

namespace LiveDomain.Core.Linq
{

    class CachingLinqCompiler<M> where M : Model
    {

        /// <summary>
        /// Bypass cache. Mainly so we can run performance tests
        /// </summary>
        public bool ForceCompilation { get; set; }

        public int CompilerInvocations { get; private set; }

        private CodeDomProvider _compiler;
        private CompilerParameters _compilerParameters;

        public const string WrapperTemplate = @"
            using System;
            using System.Linq;
            using LiveDomain.Core;

            namespace Generated
            {{
                class CompiledQuery
                {{

                    public static object QueryExpr({0} db, {1})
                    {{
                        return {2};
                    }}

                    public static object Execute(Engine<{0}> engine, params object[] args)
                    {{
                        
                        return engine.Execute(model => QueryExpr(model, {3}));
                    }}
                }}
            }}";


        Dictionary<string, MethodInfo> _queryCache = new Dictionary<string, MethodInfo>();

        public CachingLinqCompiler()
        {
            InitializeCompiler();
        }

        public MethodInfo GetCompiledQuery(string query, object[] args)
        {
            if (ForceCompilation || !_queryCache.ContainsKey(query))
            {
                _queryCache[query] = CompileQuery(query, args);
            }
            return _queryCache[query];
        }

        private MethodInfo CompileQuery(string query, object[] args)
        {
            string modelTypeName = typeof (M).FullName;
            string argsDeclaration = BuildArgsDeclaration(args);
            string argsInvocation = BuildArgsInvocation(args);
            string code = String.Format(WrapperTemplate, modelTypeName, argsDeclaration, query, argsInvocation);
#if DEBUG
            Console.WriteLine("-------------------- Begin generated code ----------------------------------------");
            Console.WriteLine(code);
            Console.WriteLine("-------------------- End generated code ----------------------------------------");
#endif
            var assembly = CompileCode(code);
            return assembly.GetType("Generated.CompiledQuery").GetMethod("Execute");
        }

        private string BuildArgsInvocation(object[] args)
        {
            List<string> castExpressions = new List<string>(args.Length);
            int idx = 0;
            foreach (object arg in args)
            {
                string typeName = arg.GetType().FullName;
                string castExpression = String.Format("({0})args[{1}]", typeName, idx++);
                castExpressions.Add(castExpression);
            }
            return String.Join(", ", castExpressions);
        }


        private string BuildArgsDeclaration(object[] args)
        {
            List<string> declarations = new List<string>(args.Length);
            int idx = 0;
            foreach (object arg in args)
            {
                string fullTypeName = arg.GetType().FullName;
                declarations.Add(fullTypeName + " @arg" + idx++ );
            }
            return String.Join(", ", declarations);
        }


        private void InitializeCompiler()
        {
            // set interface for compilation
            _compiler = new CSharpCodeProvider();
            

            // Define parameters to invoke a compiler
            _compilerParameters = new CompilerParameters();


            _compilerParameters.GenerateExecutable = false;
            _compilerParameters.GenerateInMemory = true;
            //_compilerParameters.ReferencedAssemblies.Add("System.Core.dll");

            //reference the assembly where the model is defined
            _compilerParameters.ReferencedAssemblies.Add(typeof(M).Assembly.Location);

            //reference all the assemblies referenced by the model assembly avoiding duplicaties
            foreach (AssemblyName referencedAssembly in typeof(M).Assembly.GetReferencedAssemblies())
            {
                if(!_compilerParameters.ReferencedAssemblies.Contains(referencedAssembly.Name))
                {
                    string candidateAssemblyLocation = Assembly.Load(referencedAssembly).Location;
                    if(!_compilerParameters.ReferencedAssemblies.Contains(candidateAssemblyLocation))
                    {
                        _compilerParameters.ReferencedAssemblies.Add(candidateAssemblyLocation);
                    }
                }

                
            }

#if DEBUG
            _compilerParameters.IncludeDebugInformation = true;
#endif
            _compilerParameters.WarningLevel = 3;
            _compilerParameters.TreatWarningsAsErrors = false;
            _compilerParameters.CompilerOptions = "/optimize";

            _compilerParameters.TempFiles = new TempFileCollection(".", keepFiles:false);
        }

        /// <summary>
        /// Function to compile .Net C#/VB source codes at runtime
        /// </summary>
        /// <param name="source">C# or VB source code as a string</param>
        private Assembly CompileCode(string source)
        {
            CompilerInvocations++;
            string _Errors = String.Empty;

            try
            {
                var compilerResults = 
                    _compiler.CompileAssemblyFromSource(_compilerParameters, source);

                if (compilerResults.Errors.Count > 0)
                {
                    // Return compilation errors
                    foreach (CompilerError compilerError in compilerResults.Errors)
                    {
                        _Errors += "Line number " + compilerError.Line +
                                    ", Error Number: " + compilerError.ErrorNumber +
                                    ", '" + compilerError.ErrorText + ";\r\n\r\n";
                    }

                    throw new Exception(_Errors);
                }
                return compilerResults.CompiledAssembly;
            }
            catch (Exception)
            {
                throw;
            }

        }
    }
}
