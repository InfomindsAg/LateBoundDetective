using FluentAssertions;
using LateBoundDetective.Analyzers;
using LateBoundDetective.CacheObjects;
using XSharp.VsParser.Helpers.ClassHierarchy;
using XSharp.VsParser.Helpers.Parser;

namespace LateBoundDetective.Tests;

public class AssignLocalObjectVarAnalyzerTests
{
    private AnalyzerFileResult executeAnalyzer(string sourceCode)
    {
        const string projectPath = "TestProject\\xsharpTest.xsproj";
        var classHierarchy = new ClassHierarchy("dummyCache");
        var analyzer = new AssignLocalObjectVarAnalyzer(classHierarchy, projectPath);
        var parserHelper = ParserHelper.BuildWithVoDefaultOptions();
        parserHelper.ParseText(sourceCode, projectPath);

        var result = new AnalyzerFileResult();
        analyzer.Execute("dummyFile", parserHelper.Tree, result);

        return result;
    }

    [Fact]
    public void AssignLocalObjectVarAnalyzer_LocalCtor()
    {
        var result = executeAnalyzer("""
                                     class dummy
                                         method dummy() as void
                                             local xx, yy as object
                                             xx := SqlSelectBase{}
                                             yy := SafeCreateInstance(#SqlSelectBase)
                                             return
                                     end class
                                     """);
        result.Should().NotBeNull();
        result.Items.Should().ContainSingle();
        result.Items[0].Message.Should()
            .Be("Instance of class SqlSelectBase assigned to local variable xx of type object (single assignment)");
    }

    [Fact]
    public void AssignLocalObjectVarAnalyzer_LocalCtorNested()
    {
        var result = executeAnalyzer("""
                                     class dummy
                                     METHOD __BuildDataField( a AS ARRAY ) AS DataField
                                     LOCAL oRet AS OBJECT
                                     oRet := nil

                                     IF IsArray( a )
                                     	oRet := DataField{ a[ DBS_NAME ], FieldSpec{ a[ DBS_NAME ], a[ DBS_TYPE ],  ;
                                     		a[ DBS_LEN ], a[ DBS_DEC ] } }
                                     ENDIF

                                     RETURN oRet
                                     end class
                                     """);
        result.Should().NotBeNull();
        result.Items.Should().ContainSingle();
        result.Items[0].Message.Should()
            .Be("Instance of class DataField assigned to local variable oRet of type OBJECT (single assignment)");
    }

    [Fact]
    public void AssignLocalObjectVarAnalyzer_LocalCtorMixedCtors()
    {
        var result = executeAnalyzer("""
                                     class dummy
                                     METHOD __BuildDataField( a AS ARRAY ) AS DataField
                                     LOCAL oRet AS OBJECT

                                     IF IsArray( a )
                                     	oRet := DataField{}
                                     else
                                        oRet := DataServer{}
                                     ENDIF

                                     RETURN oRet
                                     end class
                                     """);
        result.Should().NotBeNull();
        result.Items.Should().BeEquivalentTo(new[]
        {
            new
            {
                Message =
                    "Instance of class DataField assigned to local variable oRet of type OBJECT (mixed, 2 ctors)"
            },
            new
            {
                Message =
                    "Instance of class DataServer assigned to local variable oRet of type OBJECT (mixed, 2 ctors)"
            }
        });
    }

    [Fact]
    public void AssignLocalObjectVarAnalyzer_LocalCtorMixedWithOtherAssignments()
    {
        var result = executeAnalyzer("""
                                     class dummy
                                     METHOD __BuildDataField( a AS ARRAY ) AS DataField
                                     LOCAL oRet AS OBJECT

                                     IF IsArray( a )
                                     	oRet := DataField{}
                                     elseif IsSymbol(a)
                                        oRet := SafeCreateInstance(a)
                                     else
                                        oRet := self:LastValue
                                     ENDIF

                                     RETURN oRet
                                     end class
                                     """);
        result.Should().NotBeNull();
        result.Items.Should().BeEquivalentTo(new[]
        {
            new
            {
                Message =
                    "Instance of class DataField assigned to local variable oRet of type OBJECT (mixed, 1 ctors and 2 other)"
            },
        });
    }
}