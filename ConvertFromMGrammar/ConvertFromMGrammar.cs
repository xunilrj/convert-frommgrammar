using Microsoft.M;
using System;
using System.Collections.Generic;
using System.Dataflow;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace ConvertFromMGrammar
{
    [Cmdlet("Convert", "FromMGrammar")]
    public class ConvertFromMGrammar : Cmdlet
    {
        [Parameter(Position = 0)]
        public object Grammar { get; set; }

        [Parameter(Position = 1)]
        public string Languagename { get; set; }

        [Parameter(ValueFromPipeline = true)]
        public string[] Text { get; set; }

        Parser Parser;

        protected override void BeginProcessing()
        {
            string sourceItemName = Languagename;

            var grammarStr = Grammar.ToString();

            if (Grammar is FileInfo)
            {
                using (var stream = (Grammar as FileInfo).OpenRead())
                {
                    using (var reader = new StreamReader(stream))
                    {
                        grammarStr = reader.ReadToEnd();
                    }
                }
            }
            else if (Grammar is string)
            {
                if (File.Exists(Grammar.ToString()))
                {
                    grammarStr = File.ReadAllText(Grammar.ToString());
                }
                else
                {
                    grammarStr = Grammar.ToString();
                }
            }

            var options = new CompilerOptions()
            {
                Sources = {
                        new TextItem {
                            Name = sourceItemName,
                            ContentType = TextItemType.MGrammar,
                            Reader = new StringReader(grammarStr)
                        }
                    }
            };

            var results = Compiler.Compile(options);
            Parser = results.ParserFactories[sourceItemName].Create();
        }

        protected override void ProcessRecord()
        {
            foreach (var item in Text)
            {
                using (var codeStream = new StringTextStream(item))
                {
                    var result = Parser.Parse(codeStream, ErrorReporter.Standard);
                    var builder = new GraphBuilder();

                    var obj = ToAst(result, builder, new Dictionary<string, Type>());
                    WriteObject(obj);
                }
            }
        }


        PSObject ToAst(object grammarObject, IGraphBuilder graph, IDictionary<string, Type> typeMap)
        {
            var psobj = PSObject.AsPSObject(new object());

            FillObject(grammarObject, graph, typeMap, psobj);

            return psobj;
        }

        private void FillObject(object grammarObject, IGraphBuilder graph, IDictionary<string, Type> typeMap, PSObject parentObject)
        {
            var successors = graph.GetSuccessors(grammarObject);

            foreach (var successor in successors)
            {
                var propertyName = graph.GetSequenceLabel(successor).ToString();

                if (graph.GetSuccessors(successor).Count() == 0)
                {
                    continue;
                }

                var childGrammarObject = graph.GetSuccessors(successor).First();

                //if (propertyInfo.PropertyType == typeof(int))
                //{
                //    ExtractInt(parentObject, propertyInfo, childGrammarObject);
                //}
                //else if (propertyInfo.PropertyType == typeof(decimal?))
                //{
                //    ExtractDecimal(parentObject, propertyInfo, childGrammarObject);
                //}
                //else if (propertyInfo.PropertyType == typeof(Guid))
                //{
                //    ExtractGuid(parentObject, propertyInfo, childGrammarObject);
                //}
                //else if (propertyInfo.PropertyType == typeof(object))
                //{
                //    ExtractObject(parentObject, propertyInfo, childGrammarObject);
                //}
                //else if (propertyInfo.PropertyType == typeof(string))
                //{
                ExtractString(parentObject, propertyName, childGrammarObject);
                //}
                //else if (propertyInfo.PropertyType == typeof(bool))
                //{
                //    ExtractBool(parentObject, propertyInfo, childGrammarObject);
                //}
                //else if (propertyInfo.PropertyType.Name == "List`1")
                //{
                //    var list = propertyInfo.GetValue(parentObject, null);
                //    ExtractList(graph, typeMap, list, propertyInfo, childGrammarObject);
                //}
                //else if (typeof(IEnumerable<object>).IsAssignableFrom(propertyInfo.PropertyType))
                //{
                //    ExtractObjectArray(graph, parentObject, propertyInfo, childGrammarObject);
                //}
                //else
                //{
                //    var childGrammarObjectLabel = graph.GetSequenceLabel(childGrammarObject).ToString().ToLower();

                //    if (typeMap.ContainsKey(childGrammarObjectLabel) == false)
                //    {
                //        throw new TypeNotFoundParsingException();
                //    }

                //    var contentObj = Activator.CreateInstance(typeMap[childGrammarObjectLabel]);
                //    propertyInfo.SetValue(parentObject, contentObj, null);

                //    FillObject(childGrammarObject, graph, typeMap, contentObj);

                //    propertyInfo.SetValue(parentObject, contentObj, null);
                //}
            }
        }

        //private void ExtractDecimal(object parentObject, System.Reflection.PropertyInfo propertyInfo, object childGrammarObject)
        //{
        //    var contentObj = decimal.Parse(childGrammarObject.ToString(), ParseCulture);
        //    propertyInfo.SetValue(parentObject, contentObj, null);
        //}

        //private void ExtractGuid(object parentObject, System.Reflection.PropertyInfo propertyInfo, object childGrammarObject)
        //{
        //    var contentObj = new Guid(childGrammarObject.ToString());
        //    propertyInfo.SetValue(parentObject, contentObj, null);
        //}

        //private static void ExtractObjectArray(IGraphBuilder graph, object parentObject, System.Reflection.PropertyInfo propertyInfo, object childGrammarObject)
        //{
        //    var contentObj = graph.GetSequenceElements(childGrammarObject).ToArray();

        //    propertyInfo.SetValue(parentObject, contentObj, null);
        //}

        //private void ExtractList(IGraphBuilder graph, IDictionary<string, Type> typeMap, object parentObject, System.Reflection.PropertyInfo propertyInfo, object childGrammarObject)
        //{
        //    foreach (var item in graph.GetSequenceElements(childGrammarObject))
        //    {
        //        var childGrammarObjectLabel = graph.GetSequenceLabel(item).ToString().ToLower();

        //        if (typeMap.ContainsKey(childGrammarObjectLabel) == false)
        //        {
        //            throw new TypeNotFoundParsingException();
        //        }

        //        var contentObj = Activator.CreateInstance(typeMap[childGrammarObjectLabel]);
        //        FillObject(item, graph, typeMap, contentObj);

        //        (parentObject as IList).Add(contentObj);
        //    }
        //}

        private static void ExtractString(PSObject parentObject, string name, object childGrammarObject)
        {
            var contentObj = childGrammarObject.ToString();

            parentObject.Properties.Add(new PSNoteProperty(name, contentObj));
        }

        //private static void ExtractObject(object parentObject, System.Reflection.PropertyInfo propertyInfo, object childGrammarObject)
        //{
        //    var contentObj = childGrammarObject;

        //    propertyInfo.SetValue(parentObject, contentObj, null);
        //}

        //private void ExtractInt(object parentObject, System.Reflection.PropertyInfo propertyInfo, object childGrammarObject)
        //{
        //    var contentObj = int.Parse(childGrammarObject.ToString(), ParseCulture);
        //    propertyInfo.SetValue(parentObject, contentObj, null);
        //}

        //private void ExtractBool(object parentObject, System.Reflection.PropertyInfo propertyInfo, object childGrammarObject)
        //{
        //    var contentObj = bool.Parse(childGrammarObject.ToString());
        //    propertyInfo.SetValue(parentObject, contentObj, null);
        //}
    }
}
