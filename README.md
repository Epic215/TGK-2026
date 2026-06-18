# Hexfire

**Bullet hell w opuszczonym zamku.** Wcielasz się w maga, walczysz z duchami i mimikami, zbierasz magiczne bronie i stawiasz czoła wielkiemu złotemu mimikowi — strażnikowi ostatniej sali.

Dokumentacja projektu gry.
---

## 1. Krótki opis gry

**Tytuł:** Hexfire

**Koncepcja:** Gra **3D bullet hell / twin-stick shooter** z perspektywy **top-down**. Gracz wciela się w **maga** i przemierza **zamek** pełen wrogów. Celem jest przetrwanie, zdobycie broni magicznych i pokonanie **wielkiego złotego mimika** — bossa kończącego rozgrywkę.

**Inspiracje:**
- **Enter the Gungeon** — arena, zbieranie broni, dash z i-frames, HUD ze slotami ekwipunku
- Gatunek **roguelite / arena shooter** — szybka akcja, uniki, różne typy broni
- Estetyka magii i efektów VFX (ogień, tarcze, pociski)

**Co wyróżnia Hexfire:**

- **Mag jako postać grywalna**
  - Animacje: chód, atak, obrona, śmierć
  - Ruch WASD, celowanie myszą, dash ze spacją (i-frames)
  - HP i mana z regeneracją

- **System broni (5 typów, 3 sloty ekwipunku)**
  - Każda broń: osobny atak **LPM** i umiejętność **PPM**
  - Zielona kula ognia, trzy kostury (rubinowy, szafirowy, szmaragdowy), miecz lodowy
  - Wzorce strzału: single, spread, chaos, serpentine, nova

- **Przeciwnicy — duchy i mimiki**
  - **Duchy** — latające istoty ostrzeliwujące z dystansu
  - **Mimiki** — udają skrzynie i przedmioty, atakują po zbliżeniu
  - **Skrzynie-mimiki** — wariant z własnymi animacjami otwarcia

- **Dwa poziomy AI wrogów**
  - **Podstawowy** — gonienie, strafe, pojedyncze strzały, burst, spirala
  - **Rozszerzony** — okrążanie, szarże, kiting; 11 wzorców ataku (pierścień, wachlarz, krzyż, fale itd.)
  - Pasek HP nad głową każdego wroga

- **Boss — wielki złoty mimik**
  - Fazy walki zależne od HP
  - Skoki, teleport, mandala pocisków, spirale
  - Pokonanie bossa = wygrana

- **Pełna pętla gry**
  - Menu główne, pauza, game over, ekran zwycięstwa
  - HUD: HP, mana, dash, sloty broni, opis aktywnej broni

---

## 2. Użyte narzędzia

| Element | Wybór |
|--------|--------|
| Silnik | **Unity 6** (6000.3.11f1) |
| Pipeline renderowania | **URP** (Universal Render Pipeline) |
| Język skryptowy | **C#** |
| System wejścia | **Unity Input System** |
| Edytor UI | Unity UI (uGUI) + **TextMeshPro** |
| Platforma docelowa | **PC (Windows)** |

---

## 3. Opis mechaniki gry

### Świat i kamera

- **2.5D / 3D top-down** — otoczenie i postacie w 3D, rozgrywka jak w strzelance z góry
- **Zamek** — korytarze, sale walki, przeciwnicy i pickupy broni
- Kamera podąża za graczem

### Postać gracza (mag)

- **Ruch:** WASD
- **Celowanie:** obrót w stronę kursora myszy
- **Dash (Spacja):** szybki przeskok z cooldownem i krótką niewrażliwością na obrażenia
- **HP:** 100 | **Mana:** 100 (regeneracja w czasie)
- **Ekwipunek:** 3 sloty broni; podnoszenie **E**, przełączanie **1 / 2 / 3**
- Start z **zieloną kulą ognia**; na mapie leżą kostury i miecz lodowy

### System walki i bronie

| Broń | LPM | PPM |
|------|-----|-----|
| Zielona kula ognia | Pojedynczy pocisk (0 MP) | Leczenie (+HP, koszt many) |
| Kostur rubinowy | Spread — 3 pociski naraz (mana za cały strzał) | Odnowa many + efekt VFX |
| Kostur szafirowy | Chaos — seria pocisków pod losowym kątem | Shotgun — wiele pocisków (wysoki koszt MP) |
| Miecz lodowy | Atak melee w zasięgu | Magiczna tarcza (i-frames + VFX) |
| Kostur szmaragdowy | Serpentine — dwa pociski wijące się naprzemiennie (10 MP) | Verdant Nova — pierścień 18 pocisków wokół gracza (30 MP) |

- Pociski gracza mają kolizję i zadają obrażenia wrogom
- Kostury strzelają kolorowymi pociskami (pomarańcz / niebieski / zielony), odróżnionymi od czerwonych pocisków wrogów

### Przeciwnicy i boss

**Duchy**
- Latające wrogowie w korytarzach zamku
- Gonienie gracza lub ostrzał z dystansu
- Ataki: pojedyncze pociski, seria (burst), spirala

**Mimiki**
- Udają skrzynie i obiekty — atak po zbliżeniu gracza
- Zwykłe mimiki i **skrzynie-mimiki** (animacja otwarcia paszczy)
- Różne zachowania: gonienie, strafe, ostrzał z miejsca

**AI — dwa poziomy**

| Poziom | Opis |
|--------|------|
| Podstawowy | Gonienie, stanie w miejscu, strafe; atak: single, burst, spirala |
| Rozszerzony | Cofanie, okrążanie, szarże, utrzymywanie dystansu; 11 wzorców ataku (ring, fan, cross, wave, shotgun, pulse ring itd.) |

- Każdy wróg: pasek HP nad głową
- Rozszerzone AI: przewidywanie ruchu gracza, animacje Haon

**Boss — wielki złoty mimik**
- Najtrudniejszy przeciwnik w ostatniej komnacie
- Fazy walki (skoki, teleport, mandala, spirale)
- Zniszczenie bossa kończy grę zwycięstwem

### Interfejs (HUD)

- Paski **HP** i **many** + tekst wartości
- **Ring cooldownu** dasha
- **Pasek 3 slotów** broni z ikonami
- Przycisk **„i”** — opis LPM/PPM aktywnej broni

### Menu i pętla gry

- **Menu główne** — start gry, wyjście
- **Pauza (ESC)** — wznowienie, powrót do menu
- **Game Over** — po śmierci: Retry (przeładowanie sceny), Exit (menu)
- **Wygrana** — po bossie: panel zwycięstwa z tymi samymi opcjami

### Sterowanie

| Akcja | Klawisz |
|--------|---------|
| Ruch | W A S D |
| Celowanie | Mysz |
| Atak / umiejętność LPM | Lewy przycisk myszy |
| Umiejętność PPM | Prawy przycisk myszy |
| Dash | Spacja |
| Slot broni | 1 / 2 / 3 |
| Podnieś broń | E |
| Pauza | Escape |
| Q | Upuszczenie broni |

---

## 4. Użyte assety

Większość grafiki, modeli, animacji, efektów i UI pochodzi z **Unity Asset Store**.

| Kategoria | Pakiet / folder | Uwagi |
|-----------|-----------------|--------|
| Postać maga | WizardPBR | Model, animacje, kostury |
| Przeciwnicy | Haon SD Creature Pack | Duchy, mimiki, skrzynie, animacje |
| Efekty magiczne | Hovl Studio | VFX, aury, tarcza |
| Pociski / VFX | Unique_Projectiles_Volume_2 | Cząsteczki pocisków (URP) |
| Otoczenie | prefaby sceny | Ściany, korytarze, sale zamku |
| UI | Bloodlines Dark UI + prefaby Hexfire | Menu, HUD |

**Linki:**

- [WizardPBR](https://assetstore.unity.com/packages/3d/characters/humanoids/fantasy/battle-wizard-pbr-127652)
- [Haon SD Creature Pack](https://assetstore.unity.com/packages/3d/characters/creatures/haon-sd-creature-pack-311173)
- [Pixel Art Icon Pack RPG](https://assetstore.unity.com/packages/2d/gui/icons/pixel-art-icon-pack-rpg-158343)
- [Bloodlines Dark UI](https://assetstore.unity.com/packages/2d/gui/bloodlines-dark-ui-328721)
- [Hovl Studio VFX](https://github.com/VuxDzung/SEP490_SU26_Unity/tree/main/Assets/3rdParty/VFX/Hovl%20Studio/Procedural%20fire)

Assety zaimportowane i częściowo zmodyfikowane (URP, prefaby, skrypty). System broni, AI i HUD — autorskie.

---

## 5. Wykorzystanie AI

- **Grafika menu** — część elementów interfejsu wspierana generatywnie (AI) przy projektowaniu wyglądu
- **Logika gry** — częściowy udział AI przy pisaniu i refaktoryzacji C# (system broni, HUD, AI wrogów); integracja, testy i decyzje projektowe — autor

---

## 6. Uruchomienie gry

Pobierz z release paczkę **Hellfire.zip**. Wypakuj i uruchom **Hellfire.exe**, następnie wciśnij **Start**.

**Z edytora Unity:**
1. Unity Hub → projekt `TGK-2026` (Unity **6000.3.x**, URP)
2. Scena menu: `Assets/Scenes/Menu.unity`
3. Play

---

## 7. Zrzuty ekranu

### Ekran główny
<img width="1919" height="1079" alt="image" src="https://github.com/user-attachments/assets/8a501f66-bdf5-4e70-861f-3a9ec0965441" />

### HUD + Wyglad gry
<img width="1919" height="1079" alt="image" src="https://github.com/user-attachments/assets/6a98c545-adb8-4837-b006-9fb97bf645ab" />

### Walka
<img width="1919" height="1079" alt="image" src="https://github.com/user-attachments/assets/f82fe85a-867d-425f-8377-f173eb3bf6f4" />

### Opisy broni

<img width="604" height="530" alt="image" src="https://github.com/user-attachments/assets/5a25d5c4-8fe9-4623-b60a-a5165294561b" />

### Główny boss

<img width="1919" height="1079" alt="image" src="https://github.com/user-attachments/assets/f87f38df-b2af-4586-91d5-177968ea0d19" />

### Ekran WIN

<img width="1919" height="1079" alt="image" src="https://github.com/user-attachments/assets/4cc10c71-b6e3-4bdf-bced-c566bda6a904" />






---

## Struktura kodu

```
Assets/Hexfire/     — gracz, broń, UI, rozszerzone AI, prefaby wrogów
Assets/Scripts/     — AI podstawowe, boss, menu, pauza, kamera
```
---
Link do repozytorium:
https://github.com/Epic215/TGK-2026
