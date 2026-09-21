# OpenAI Work Tank - rövid jelentés

A kiinduló kérés egy kis, hordozható Windows program volt, amely a Codex heti keretét a tálcán jelzi, és a Windows indulásakor is elindul. Elkészült egy natív .NET 8 Windows Forms tálcaalkalmazás és egy önálló telepítő.

A program a helyi Codex App Serverből olvassa az adatokat. A telepített Codex valós válaszában a `codex` csomag 300 perces és 10080 perces ablakot adott. A program a 10080 perces ablakot választotta heti keretnek, a 300 perceset csak másodlagos 5 órás értéknek.

Az eredmény két kész fájl: az önálló alkalmazás és a telepítő. A telepítő a felhasználó saját alkalmazásadat-mappájába másol, bekapcsolja az automatikus indítást, majd elindítja a programot. Rendszergazdai jog nem kell.

## v0.1.1 hibajavítás
A v0.1.0 indításkor előforduló, leválasztott Process objektumra hivatkozó hiba javítva lett. Az újracsatlakozás egyszerre csak egy időzítőt használ, a háttérből érkező állapotváltozások pedig a Windows felületi szálára kerülnek.

## v0.1.2 használhatósági javítás
A program most a Codex Desktop saját helyi CLI-fájlját is felismeri. A tálcaikonban a százalék nagyobb számokkal látszik, a részletablak a jobb alsó tálca mellé került.

## 2026-09-21 - Nyilvános GitHub-előkészítés
A forráskód nyilvános megosztásra lett előkészítve. A felhasználó a nyilvános láthatóságot és a GNU GPL v3 licencet választotta. A telepítő- és alkalmazásfájlok méretük miatt nem kerülnek a Git előzményeibe; ezek kiadási csatolmányként kezelhetők.
