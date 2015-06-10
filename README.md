# Convert-FromMGrammar

A long ago, Microsoft killed a project called Oslo. Inside the project there is yet another parse generator called MGrammar. 
I still find MGrammar one of the easiest parser generator that exists.

Support the idea to revive Oslo: http://www.cazzulino.com/opensource-m.html

Powrshell:
```powershell
Import-Module "E:\GitHub\convert-frommgrammar\ConvertFromMGrammar\bin\Debug\System.DataFlow.dll"
Import-Module "E:\GitHub\convert-frommgrammar\ConvertFromMGrammar\bin\Debug\Microsoft.M.dll"
Import-Module "E:\GitHub\convert-frommgrammar\ConvertFromMGrammar\bin\Debug\Xaml.dll"
Import-Module "E:\GitHub\convert-frommgrammar\ConvertFromMGrammar\bin\Debug\ConvertFromMGrammar.dll"

$l = 'module MyParsers
{
language DirectionLang
{
    token Letter = "a".."z" | "A".."Z";
    token Id = Letter+;
    syntax Main = l:Id "->" r:Id => {Source => l, Destination => r };
}
}'
@("a -> b","b -> c","c -> d") | Convert-FromMGrammar -Grammar $l -Languagename "MyParsers.DirectionLang"
```
Result:
```
Source Destination
------ -----------
a      b          
b      c          
c      d    
```
