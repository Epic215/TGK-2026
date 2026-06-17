# Hexfire

**Bullet hell w opuszczonym zamku.** Wcielasz się w maga, walczysz z duchami i mimikami, zbierasz magiczne bronie i stawiasz czoła wielkiemu złotemu mimikowi — strażnikowi ostatniej sali.

---

## 1. O grze

**Hexfire** to **3D twin-stick shooter** z widokiem **top-down**. Przemierzasz mroczne korytarze i sale zamku, unikasz gradu pocisków, zarządzasz maną i budujesz arsenał z trzech slotów broni. Każda broń ma osobny atak **LPM** i umiejętność **PPM**. Run kończy się zwycięstwem po pokonaniu **bossa wielkiego złotego mimika** albo porażką, gdy zabraknie Ci życia.

**Inspiracje:** *Enter the Gungeon*, klasyczne arena shootery i bullet helle — szybka akcja, dash z krótką niewrażliwością, czytelny HUD.

**Co wyróżnia Hexfire:**
- Mag z pełnymi animacjami — chód, atak, obrona, śmierć.
- **Pięć unikalnych broni** z odmienionymi wzorcami strzału: od prostej kuli ognia po serpentine i pierścień nova.
- **Różnorodni przeciwnicy** — wiszące **duchy** i podstępne **mimiki** (w tym skrzynie-kreatury), każdy ze swoim stylem walki.
- **Dwupoziomowe AI** — od prostych strzelców po zaawansowane wzorce ataku z okrążaniem, szarżami i wachlarzami pocisków.
- **Boss** — wielki złoty mimik z fazami walki, skokami, teleportem i spiralami pocisków.

---

## 2. Świat i rozgrywka

### Zamek

Gra rozgrywa się w **3D z perspektywy z góry** — widzisz korytarze, komnaty i przeciwników na planszy, jak w klasycznym bullet hellu. Kamera podąża za magiem; ściany zamku ograniczają pole walki i zmuszają do ruchu bokiem, cofania i precyzyjnych uników.

### Mag — sterowanie i zasoby

| Mechanika | Opis |
|-----------|------|
| Ruch | **WASD** |
| Celowanie | Mysz — postać obraca się w stronę kursora |
| Dash | **Spacja** — szybki przeskok z cooldownem i krótkimi **i-frames** |
| HP | 100 punktów życia |
| Mana | 100 punktów, regeneruje się w czasie |
| Broń | 3 sloty — przełączanie **1 / 2 / 3**, podnoszenie z ziemi **E** |

Startujesz z **zieloną kulą ognia**; po drodze znajdziesz kostury, miecz lodowy i inne pickupy ukryte w zamku.

---

## 3. Broń

Każda broń to osobny zestaw umiejętności. Mana jest liczona za cały strzał lub za użycie PPM — zależnie od broni.

| Broń | LPM | PPM |
|------|-----|-----|
| **Zielona kula ognia** | Pojedynczy pocisk (0 MP) | Leczenie — odzyskanie HP za manę |
| **Kostur rubinowy** | Spread — 3 pociski naraz | Odnowa many + efekt VFX |
| **Kostur szafirowy** | Chaos — seria losowych pocisków | Shotgun — gęsty wachlarz (wysoki koszt MP) |
| **Miecz lodowy** | Cios mieczem w zasięgu | Magiczna tarcza — i-frames i aura ochronna |
| **Kostur szmaragdowy** | Serpentine — dwa pociski wijące się naprzemiennie | Verdant Nova — pierścień 18 pocisków wokół gracza |

Wzorce strzału gracza obejmują m.in. **single**, **spread**, **chaos** i **serpentine** — od precyzyjnych salw po chaotyczne i falujące ataki w stylu klasycznych bullet helli.

---

## 4. Przeciwnicy

Zamek tętni życiem — i nie zawsze jest ono przyjazne. Na gracza czyhają dwa typy istot oraz boss.

### Duchy

Eteryczne wrogowie unoszące się nad podłożem. Atakują z dystansu, gonią maga po korytarzach lub utrzymują pozycję i zasypują gradem czerwonych pocisków. Dobrze sprawdzają się w ciasnych przejściach, gdzie trudniej o pełny unik.

### Mimiki

Podstępne istoty udające skrzynie, beczki i zwykłe obiekty — gdy się zbliżysz, **otwierają paszczę i ostrzeliwują**. Wśród nich są zwykłe mimiki oraz **skrzynie-mimiki** z własnymi animacjami. Różnią się zachowaniem: jedne gonią, inne strzelają z miejsca lub manewrują na boki.

### Dwa poziomy sztucznej inteligencji

| Poziom | Kto | Zachowanie |
|--------|-----|------------|
| **Podstawowy** | Większość duchów i mimików na mapie | Gonienie, strafe, pojedyncze strzały, seria (burst) lub spirala pocisków |
| **Rozszerzony** | Wybrani przeciwnicy (m.in. zaawansowane mimiki) | Cofanie, okrążanie, szarże, utrzymywanie dystansu; ataki: pierścień, wachlarz, krzyż, fale, shotgun, pulsujące pierścienie — **11 wzorców** konfigurowanych per wróg |

Każdy wróg ma **pasek HP** nad głową. Silniejsze jednostki przewidują ruch gracza i łączą ruch z ostrzałem w złożone sekwencje.

### Boss — wielki złoty mimik

Strażnik ostatniej komnaty. Walka toczy się w **fazach** — im mniej ma HP, tym agresywniej atakuje: skoki, teleport, mandala pocisków, spirale. Pokonanie bossa kończy grę zwycięstwem.

---

## 5. Interfejs i pętla gry

- **HUD** — paski HP i many, ring cooldownu dasha, 3 sloty broni z ikonami.
- **Panel „i”** — opis LPM/PPM aktywnej broni.
- **Menu główne** — start, wyjście.
- **Pauza (ESC)** — wznowienie lub powrót do menu.
- **Game Over** — po śmierci: Retry (od nowa) lub Exit (menu).
- **Wygrana** — po bossie: ekran zwycięstwa z tymi samymi opcjami.

### Sterowanie — skrót

| Akcja | Klawisz |
|--------|---------|
| Ruch | W A S D |
| Celowanie | Mysz |
| Atak LPM | LPM |
| Umiejętność PPM | PPM |
| Dash | Spacja |
| Slot broni | 1 / 2 / 3 |
| Podnieś broń | E |
| Pauza | Escape |

---

## 6. Narzędzia

| Element | Wybór |
|--------|--------|
| Silnik | Unity 6 (6000.3.11f1) |
| Renderowanie | URP |
| Język | C# |
| Wejście | Unity Input System |
| UI | uGUI + TextMeshPro |
| Platforma | PC (Windows) |

---

## 7. Assety

Grafika, modele, animacje i efekty pochodzą głównie z **Unity Asset Store**, dopasowane do URP.

| Kategoria | Pakiet |
|-----------|--------|
| Mag | [Battle Wizard PBR](https://assetstore.unity.com/packages/3d/characters/humanoids/fantasy/battle-wizard-pbr-127652) |
| Duchy i mimiki | [Haon SD Creature Pack](https://assetstore.unity.com/packages/3d/characters/creatures/haon-sd-creature-pack-311173) |
| Ikony broni | [Pixel Art Icon Pack RPG](https://assetstore.unity.com/packages/2d/gui/icons/pixel-art-icon-pack-rpg-158343) |
| UI menu | [Bloodlines Dark UI](https://assetstore.unity.com/packages/2d/gui/bloodlines-dark-ui-328721) |
| VFX | Hovl Studio, Unique Projectiles Vol. 2 |

Modele przeciwników i animacje (duchy, mimiki, skrzynie) pochodzą z pakietu Haon. Skrypty gry, system broni, AI i HUD — **autorskie**.

---

## 8. Wykorzystanie AI (narzędzia generatywne)

- **Grafika menu** — część elementów UI wspierana generatywnie przy projektowaniu wyglądu.
- **Kod** — częściowa pomoc AI przy pisaniu i refaktoryzacji C# (broń, HUD, AI); integracja, balans i decyzje projektowe — autor.

---

## 9. Uruchomienie

**Wersja do grania:** pobierz z release paczkę **Hellfire.zip**, wypakuj i uruchom **Hellfire.exe**, następnie wciśnij **Start**.

**Z Unity (dla developerów):**
1. Unity Hub → projekt `TGK-2026` (Unity 6000.3.x, URP).
2. Scena menu: `Assets/Scenes/Menu.unity`.
3. Play.

---

## 10. Zrzuty ekranu

*Do uzupełnienia — walka z duchami, mimikami, boss i HUD.*

---

## Struktura projektu

```
Assets/Hexfire/     — gracz, broń, UI, rozszerzone AI, prefaby wrogów
Assets/Scripts/     — AI podstawowe, boss, menu, pauza, kamera
```

