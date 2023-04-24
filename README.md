# How to run

## to generate the parser from the grammar

```commandline
antlr4 SystemVerilogLexer.g4 -Dlanguage='CSharp' -no-listener -visitor

antlr4 SystemVerilogParser.g4 -Dlanguage='CSharp' -no-listener -visitor
```

## to see the parse tree

```commandline
grun antlr.SystemVerilogParser svprogram exampleFile -gui &
```
