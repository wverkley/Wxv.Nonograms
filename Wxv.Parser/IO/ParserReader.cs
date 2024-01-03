namespace Wxv.Parser.IO;

public class ParserReader
{
    private static readonly Lazy<ParserEngine> LazyBnfParser = new(CreateBnfParser);
    public static ParserEngine BnfParser { get { return LazyBnfParser.Value; } }

    private static ParserEngine CreateBnfParser()
    {
        var builder = new ParserBuilder();

        builder.Symbol("bnf", true)
            .Reference("whitespace", minOccurs: 0)
            .Reference("rule", maxOccurs: int.MaxValue, canFallback: false)
            .Eof();

        builder.Symbol("whitespace", isAnonymous: true)
            .Reference("whitespace-item", minOccurs: 1, maxOccurs: int.MaxValue);

        builder.Symbol("whitespace-item", isAnonymous: true)
            .Regex(@"\s+") // whitespace
            .Or()
            .Regex(@"\/\/(.*?\n)") // single line comment //
            .Or()
            .Regex(@"\/\*(.*?\*\/)"); // multi line comment /* */

        builder.Symbol("rule")
            .Reference("rule-option", minOccurs: 0)
            .Reference("rule-name", canFallback: false)
            .Reference("whitespace", minOccurs: 0)
            .Regex(@"\:\=")
            .Reference("rule-content")
            .Regex(@"\.")
            .Reference("whitespace", minOccurs: 0);

        builder.Symbol("rule-option")
            .Regex(@"\@")
            .Or()
            .Regex(@"\-");

        builder.Symbol("rule-name")
            .Regex(@"[_a-zA-Z][_\-a-zA-Z0-9]*");

        builder.Symbol("rule-content")
            .Reference("group-first")
            .Reference("group-alt", minOccurs: 0, maxOccurs: int.MaxValue)
            .Reference("whitespace", minOccurs: 0);

        builder.Symbol("is-root")
            .Regex(@"\@");

        builder.Symbol("group-first")
            .Reference("whitespace", minOccurs: 0)
            .Reference("group");

        builder.Symbol("group-alt")
            .Reference("whitespace", minOccurs: 0)
            .Regex(@"\|", canFallback: false)
            .Reference("whitespace", minOccurs: 0)
            .Reference("group");

        builder.Symbol("group")
            .Reference("group-item", maxOccurs: int.MaxValue);

        builder.Symbol("group-item")
            .Reference("whitespace", minOccurs: 0)
            .Reference("expression", canFallback: false)
            .Reference("whitespace", minOccurs: 0)
            .Reference("cardinality", minOccurs: 0)
            .Reference("whitespace", minOccurs: 0)
            .Reference("no-fallback", minOccurs: 0);

        builder.Symbol("expression")
            .Reference("ref")
            .Or()
            .Reference("regex")
            .Or()
            .Reference("rule-anon")
            .Or()
            .Reference("eof");

        builder.Symbol("ref")
            .Regex(@"[_a-zA-Z][_\-a-zA-Z0-9]*");

        builder.Symbol("regex")
            .Regex(@"(\"".*?\"")+");

        builder.Symbol("rule-anon")
            .Regex(@"\(", canFallback: false)
            .Reference("rule-content")
            .Regex(@"\)");

        builder.Symbol("eof")
            .Regex(@"\@eof");

        builder.Symbol("cardinality")
            .Regex(@"\?") // optional
            .Or()
            .Regex(@"\*") // 0 to many
            .Or()
            .Regex(@"\+") // 1 to many
            .Or()
            // exact
            .Regex(@"\[")
            .Reference("whitespace", minOccurs: 0)
            .Reference("int")
            .Reference("whitespace", minOccurs: 0)
            .Regex(@"\,")
            .Reference("whitespace", minOccurs: 0)
            .Reference("ordinal")
            .Reference("whitespace", minOccurs: 0)
            .Regex(@"\]");

        builder.Symbol("int")
            .Regex(@"[0-9]+");

        builder.Symbol("ordinal")
            .Regex(@"([0-9]+)|\*");

        builder.Symbol("no-fallback")
            .Regex(@"\>");

        return builder.Build();
    }
        
    private readonly ParserEngine _result = new();

    public ParserEngine Read(string source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var bnf = BnfParser.Parse(source);
        if (bnf == null || !bnf.IsMatch)
            throw new InvalidDataException();

        foreach (var sourceRule in bnf.Where(rr => rr.Name == "rule"))
            ReadSourceRule(sourceRule);

        // ReSharper disable once ObjectCreationAsStatement
        new ParserValidator(_result);
        return _result;

    }

    private void ReadSourceRule(ParserResult sourceRule)
    {
        var sourceRuleOption = sourceRule.FirstOrDefault(rn => rn.Name == "rule-option");
        var sourceRuleName = sourceRule.First(rn => rn.Name == "rule-name");
        var sourceRuleContent = sourceRule.First(rn => rn.Name == "rule-content");

        var isRoot = sourceRuleOption != null && sourceRuleOption.Value == "@";
        var isAnonymous = sourceRuleOption != null && sourceRuleOption.Value == "-";

        var symbol = new ParserSymbol(sourceRuleName.Value, isRoot: isRoot, isAnonymous: isAnonymous);
        _result.AddSymbol(symbol);

        //Console.WriteLine(sourceRuleName.Value);
        ReadSourceRuleContent(sourceRuleName.Value, sourceRuleContent, symbol);
    }

    private void ReadSourceRuleContent(string name, ParserResult sourceRuleContent, ParserSymbol symbol)
    {
        var sourceGroups = sourceRuleContent
            .Where(g => g.Name.StartsWith("group-"))
            .SelectMany(g => g.Where(gi => gi.Name == "group"));

        var sourceGroupIndex = 0;
        foreach (var sourceGroup in sourceGroups)
        {
            var symbolGroup = new ParserSymbolGroup();
            symbol.AddGroup(symbolGroup);

            ReadSourceGroup(name + "+" + sourceGroupIndex, sourceGroup, symbolGroup);
            sourceGroupIndex++;
        }
    }

    private void ReadSourceGroup(string name, ParserResult sourceGroup, ParserSymbolGroup symbolGroup)
    {
        //Console.WriteLine("  " + name);

        var sourceGroupItems = sourceGroup.Where(gi => gi.Name == "group-item");

        var groupItemIndex = 0;
        foreach (var sourceGroupItem in sourceGroupItems)
        {
            ReadSourceGroupItem(name + "+" + groupItemIndex, sourceGroupItem, symbolGroup);
            groupItemIndex++;
        }
    }

    private void ReadSourceGroupItem(string name, ParserResult sourceGroupItem, ParserSymbolGroup symbolGroup)
    {
        var sourceCardinality = sourceGroupItem.FirstOrDefault(e => e.Name == "cardinality");
        int minOccurs, maxOccurs;
        ReadSourceCardinality(sourceCardinality, out minOccurs, out maxOccurs);

        var sourceNoFallback = sourceGroupItem.FirstOrDefault(e => e.Name == "no-fallback");
        var canFallback = sourceNoFallback == null;

        var sourceExpression = sourceGroupItem.First(e => e.Name == "expression");

        //Console.WriteLine("    " + name + ": " + sourceExpression.First().Name);

        ParserResult sourceExpressionItem;
        if ((sourceExpressionItem = sourceExpression.FirstOrDefault(pr => pr.Name == "ref")) != null)
        {
            symbolGroup.AddItem(new ParserReference(sourceExpressionItem.Value, minOccurs, maxOccurs, canFallback));
        }
        else if ((sourceExpressionItem = sourceExpression.FirstOrDefault(pr => pr.Name == "regex")) != null)
        {
            symbolGroup.AddItem(new ParserRegex(
                sourceExpressionItem.Value.Substring(1, sourceExpressionItem.Value.Length - 2).Replace("\"\"", "\""),
                minOccurs,
                maxOccurs,
                canFallback));
        }
        else if ((sourceExpressionItem = sourceExpression.FirstOrDefault(pr => pr.Name == "rule-anon")) != null)
        {
            symbolGroup.AddItem(new ParserReference(name, minOccurs, maxOccurs, canFallback));

            var sourceRuleContent = sourceExpressionItem.First(rn => rn.Name == "rule-content");

            var anonRule = new ParserSymbol(name, false, true);
            _result.AddSymbol(anonRule);

            ReadSourceRuleContent(name, sourceRuleContent, anonRule);
        }
        // ReSharper disable once RedundantAssignment
        else if ((sourceExpressionItem = sourceExpression.FirstOrDefault(pr => pr.Name == "eof")) != null)
        {
            symbolGroup.AddItem(new ParserEof(canFallback));
        }

    }

    private void ReadSourceCardinality(ParserResult? sourceCardinality, out int minOccurs, out int maxOccurs)
    {
        minOccurs = 1;
        maxOccurs = 1;
        if (sourceCardinality == null) { }
        else if (sourceCardinality.Value == "?")
        {
            minOccurs = 0;
            maxOccurs = 1;
        }
        else if (sourceCardinality.Value == "*")
        {
            minOccurs = 0;
            maxOccurs = int.MaxValue;
        }
        else if (sourceCardinality.Value == "+")
        {
            minOccurs = 1;
            maxOccurs = int.MaxValue;
        }
        else
        {
            minOccurs = int.Parse(sourceCardinality.First(pr => pr.Name == "int").Value);
            maxOccurs = sourceCardinality.First(pr => pr.Name == "ordinal").Value == "*"
                ? int.MaxValue
                : int.Parse(sourceCardinality.First(pr => pr.Name == "ordinal").Value);
        }
    }

}