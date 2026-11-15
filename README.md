# Labb_3_QuizApp

## Om projektet

QuizApp är en frågesportsapplikation utvecklad i C# med WPF. Användaren får svara på flervalsfrågor och får direkt återkoppling på om svaret är rätt eller fel.

## Viktiga tekniska lösningar:

Applikationen använder WPF och MVVM för att skilja gränssnittet från applikationslogiken. ViewModels hanterar till exempel aktiva frågepaket och användarens svar, medan databindning kopplar dessa värden till gränssnittet. `RelayCommand` används för att koppla knapptryckningar till funktioner i ViewModels.

Frågepaket sparas och läses in som JSON med `System.Text.Json`. Det gör det möjligt att återanvända och redigera paket mellan körningar. Frågor består av en fråga, ett rätt svar och en array med tre felaktiga svar. Det går också att importera och spara packet från OpenTrivia DB.

## Problem som jag stött på under utvecklingen

När jag testade att läsa in ett frågepaket från JSON fick jag ett deserialiseringsfel för klassen `Question`. Konstruktorn tog emot tre separata felaktiga svar, medan JSON-modellen innehöll en array med namnet `IncorrectAnswer`. Därför kunde `System.Text.Json` inte koppla konstruktorns parametrar till modellens egenskaper.

Jag läste felmeddelandet och jämförde JSON-strukturen med `Question`-klassen. Lösningen blev att lägga till en konstruktor som tar emot en array med felaktiga svar och markera den med `[JsonConstructor]`. Efter ändringen kunde paketen läsas in utan att ändra JSON-strukturen.
