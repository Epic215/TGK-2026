# Hexfire

Dokumentacja projektu gry — wersja prototypowa.  
Poniższy opis jest elastyczny: niektóre elementy są już zaimplementowane, inne zaplanowane na kolejne iteracje.

---

## 1. Krótki opis gry

**Tytuł roboczy:** Hexfire  

**Koncepcja:** Prototyp **3D bullet hell / twin-stick shootera** z perspektywy **top-down**, w którym gracz wciela się w **maga**. Gra rozgrywa się na arenie z wrogami i bossem; celem runy jest przetrwanie i pokonanie przeciwników przy użyciu broni magicznych i uników.

**Inspiracje:**
- **Enter the Gungeon** — pętla walki w pokojach/arenie, zbieranie broni, dash z invincibility frames, czytelny HUD z slotami ekwipunku.
- Ogólnie gatunek **roguelite / arena shooter** (szybka akcja, uniki, różne typy broni).
- Estetyka magii i efektów VFX z pakietów Unity Asset Store (ogień, tarcze, pociski).

**Co jest charakterystyczne w tej wersji:**
- Mag z animacjami (chód, atak, obrona) zamiast „sztywnej” postaci.
- **System broni oparty na typach** (kula, kostury, miecz) z osobnymi umiejętnościami LPM/PPM — np. spread, chaos, serpentine, leczenie, odnowa many, magiczna tarcza, nova.
- Nowy kod gry w folderze `Assets/Hexfire/` (gracz, broń, UI), obok starszego prototypu w `Assets/Scripts/`.

**Stan projektu:** To **prototyp demonstracyjny**, nie pełna gra. Działa walka na jednej scenie (`SampleScene`). Do oddania zostają m.in. mapa, build `.exe`, screenshoty i linki do assetów (patrz **§9 TODO**).

---

## 2. Użyte narzędzia

| Element | Wybór |
|--------|--------|
| Silnik | **Unity 6** (6000.3.11f1) |
| Pipeline renderowania | **URP** (Universal Render Pipeline) |
| Język skryptowy | **C#** |
| System wejścia | **Unity Input System** (`InputSystem`) |
| Edytor UI | Unity UI (uGUI) + **TextMeshPro** |
| Platforma docelowa | **PC (Windows)** — projekt uruchamiany w edytorze Unity lub jako build `.exe` |

---

## 3. Opis mechaniki gry

### Świat i kamera
- **2.5D / 3D top-down** — postacie i otoczenie w 3D, rozgrywka jak w strzelance z góry (brak swobodnej kamery FPS).
- **Jedna arena / mapa ręczna** na scenie (ściany, korytarze, przestrzenie walki). Proceduralna mapa — **planowana**, niezaimplementowana.
- Kamera podąża za graczem (skrypt `CameraFollow`).

### Postać gracza (mag)
- **Ruch:** WASD, `CharacterController`, grawitacja i przyciąganie do podłoża.
- **Celowanie:** obrót w stronę kursora myszy.
- **Dash (Spacja):** szybki przeskok z cooldownem; podczas dasha **i-frames** (krótka niewrażliwość na obrażenia).
- **Atrybuty:** HP (domyślnie 100), mana (domyślnie 100, regeneracja w czasie).
- **Ekwipunek:** 3 sloty broni; podnoszenie z mapy klawiszem **E**, przełączanie **1 / 2 / 3**.

### System walki i bronie
Broń zdefiniowana jako **ScriptableObject** (`WeaponData` i klasy pochodne). Przykładowe typy w prototypie:

| Broń | LPM | PPM |
|------|-----|-----|
| Zielona kula ognia | Pojedynczy pocisk (0 MP) | Leczenie (+HP, koszt many) |
| Kostur 1 (rubinowy) | Spread — 3 pociski naraz (mana za cały strzał) | Odnowa many + efekt VFX |
| Kostur 2 (szafirowy) | Chaos — seria pocisków pod losowym kątem | „Shotgun” — wiele pocisków (wysoki koszt MP) |
| Miecz lodowy | Atak melee w zasięgu | Magiczna tarcza (i-frames + VFX) |
| Kostur 3 (szmaragdowy) | Serpentine — dwa pociski wijące się naprzemiennie (10 MP) | Verdant Nova — pierścień 18 pocisków wokół gracza (30 MP, cd ~4,5 s) |

Wzorce strzału (single / spread / chaos / serpentine) nawiązują do wcześniejszego prototypu `PlayerInventory`.

Pociski: prefaby z kolizją, obrażenia wrogów przez tag `Enemy` / komponenty zdrowia.

### Przeciwnicy i boss

W projekcie współistnieją **dwa systemy AI** — starszy (na większości wrogów na scenie) i nowy Hexfire (gotowy kod + prefab, do rozszerzenia na mapę).

#### Starszy system (`Assets/Scripts/`)

Używany przez **większość wrogów** na `SampleScene`.

| Skrypt | Rola |
|--------|------|
| `EnemyController` | Ruch + strzelanie |
| `EnemyHealth` | HP, obrażenia, śmierć |
| `BossController` | Boss — fazy, skoki, teleport, mandala, spirale |

**Ruch (`EnemyController`):** `Chase` (gonienie z zigzagiem), `Stationary` (stoi), `Strafe` (ruch na boki wokół gracza).

**Atak:** `Single` (pojedyncze pociski), `Burst` (seria pod kątem), `Spiral` (wir pocisków przez kilka sekund).

Wrog szuka gracza po tagu `Player`, obraca się w jego stronę, strzela z `firePoint` prefabem `EnemyBullet`.

#### Nowy system Hexfire (`Assets/Hexfire/Enemies/`)

Modułowy kontroler do konfiguracji **per prefab** w Inspectorze — bez zmiany kodu.

| Skrypt | Rola |
|--------|------|
| `HexfireEnemyController` | Ruch + wzorce ataku |
| `HexfireEnemyBulletSpawner` | Spawn pocisków wroga |
| `HexfireEnemyAnimator` | Animacje Haon (Ghost / Mimic / Chest Mimic) |
| `HexfireBossController` | Rozbudowany boss (fazy, mandala, skoki) — **kod gotowy**, scena na razie używa starego `BossController` |

**Tryby ruchu (`HexfireEnemyMoveMode`):**

| Tryb | Zachowanie |
|------|------------|
| `Chase` | Gonienie gracza z lekkim zigzagiem bocznym |
| `Stationary` | Bez ruchu (np. wieżyczka) |
| `Strafe` | Ruch na boki + korekta dystansu |
| `Retreat` | Cofa się, gdy gracz za blisko; podchodzi, gdy za daleko |
| `Orbit` | Okrąża gracza |
| `Kite` | Utrzymuje preferowany dystans (strzelanie z bezpiecznej odległości) |
| `Charge` | Okresowy szarż na gracza |

**Wzorce ataku (`HexfireEnemyAttackPattern`):** `Single`, `Burst`, `Spiral`, `Ring`, `Fan`, `Scatter`, `Cross`, `PulseRing`, `Alternating`, `Shotgun`, `Wave` — każdy z własnymi parametrami (cooldown, liczba pocisków, kąty itd.).

**Dodatkowo:** przewidywanie ruchu gracza (`aimLeadStrength`), opcja zatrzymania ruchu podczas ataku (`freezeMovementWhileAttacking`), pasek HP nad głową (`EnemyCanvas`).

**Prefab referencyjny:** `Assets/Hexfire/Enemies/Prefabs/EnemyNewFinal.prefab` (mimic-skrzynia + nowe skrypty). Na scenie jest **jedna** taka instancja; pozostałe wrogowie nadal na starym `EnemyController`.

#### Boss i warunek wygranej

- **Boss na scenie:** `BossController` (stary prototyp) — fazy zależne od HP, różne wzorce strzału.
- **Wygrana:** komponent `WinOnDestroy` na bossie — po zniszczeniu pokazuje panel wygranej (`GameOverMenu` w trybie win).

#### Co dalej z AI (opcjonalnie)

- Podmiana pozostałych wrogów na prefaby z `HexfireEnemyController` (różne `moveMode` + `attackPattern` per typ).
- Migracja bossa na `HexfireBossController`.
- Unikanie pocisków gracza — **niezaimplementowane**.

### Interfejs (HUD)
- Paski **HP** i **many** + tekst wartości.
- **Ring cooldownu** dasha.
- **Pasek 3 slotów** broni z ikonami; przycisk **„i”** z opisem LPM/PPM aktywnej broni.
- Skrypty w `Assets/Hexfire/UI/` (`PlayerHudWire`, `EquipmentBarHud`, `WeaponInfoPanel`).

### Menu i pętla gry
- **Menu główne** (`Menu` scena): start gry, wyjście (`MenuManager`).
- **Pauza (ESC):** panel wstrzymania, wznowienie, powrót do menu (`PauseMenu`) — działa na scenie; **do dopracowania wizualnie** (spójny styl z menu głównym).
- **Game Over:** panel po śmierci gracza (`GameOverMenu`) — przyciemnione tło, **Retry** (przeładowanie sceny) i **Exit** (menu główne).
- **Wygrana:** po zniszczeniu bossa (`WinOnDestroy`) — ten sam panel z komunikatem o wygranej.

### Sterowanie (skrót)

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

---

## 4. Użyte assety

Większość grafiki, modeli, animacji, efektów i UI pochodzi z **Unity Asset Store** (oraz domyślnych zasobów Unity / TextMeshPro).

**Źródła (szczegółowe linki — do uzupełnienia):**

| Kategoria | Pakiet / folder w projekcie | Uwagi |
|-----------|----------------------------|--------|
| Postać maga | `WizardPBR` | Model, animacje, kostury |
| Efekty magiczne | `Hovl Studio` (Magic effects pack, Procedural fire, itd.) | VFX, aury, tarcza |
| Pociski / VFX | `Unique_Projectiles_Volume_2` | Cząsteczki pocisków (wymaga URP) |
| Otoczenie / level kit | prefaby w scenie (`Wall`, `Corridor`, `EMPTY_SPACE`, itd.) | Zestawy z Asset Store / wcześniejszy import |
| UI | własne prefaby w `Assets/Hexfire/UI/` + elementy z pakietów UI | Część layoutu budowana skryptami edytora |

**Status modyfikacji:** assety głównie **zaimportowane**; część **zmodyfikowana** (materiały pod URP, prefaby broni/pickupów, przypisane skrypty Hexfire). Własne skrypty i konfiguracja broni — **autorskie**.

*Lista linków do poszczególnych stron Asset Store zostanie dodana w kolejnej wersji dokumentu.*

---

## 5. Wykorzystanie AI

- **Grafika menu** — część elementów interfejsu / koncepcji menu wspierana generatywnie (AI) przy projektowaniu wyglądu.
- **Logika gry** — **niewielki** udział AI przy pomocy w pisaniu i refaktoryzacji fragmentów kodu C# (system broni, HUD, setup gracza); ostateczna integracja, testy i decyzje projektowe — autor.

---

## 6. Uruchomienie gry

*Sekcja do uzupełnienia przez autora (build wykonywalny + krótka instrukcja, jeśli potrzebna).*

**Minimalnie z projektu Unity:**
1. Unity Hub → projekt `TGK-2026` (Unity **6000.3.x** z modułem URP).
2. Scena startowa menu: `Assets/Scenes/Menu.unity` (lub bezpośrednio `SampleScene.unity` do testów walki).
3. Play w edytorze.

---

## 7. Zrzuty ekranu

*Do uzupełnienia — jeden lub więcej reprezentatywnych screenshotów (walka, HUD, menu, boss).*

---

## 8. Bibliografia

*Opcjonalnie — do uzupełnienia, jeśli będą cytowane algorytmy lub źródła techniczne.*

---

## Struktura kodu (skrót)

```
Assets/Hexfire/
├── Core/          — wspólne interfejsy (np. IDamageable)
├── Player/        — ruch, dash, HP, mana, ekwipunek, prefab Player_Mage
├── Weapons/       — broń, pociski, pickupy, dane ScriptableObject
├── Enemies/       — HexfireEnemyController, prefaby wrogów, animator
├── UI/            — HUD, pasek broni, panel informacji, game over / win
└── Editor/        — menu pomocnicze Hexfire (setup maga, animacje, materiały URP)

Assets/Scripts/    — starszy prototyp (wrogowie, boss, menu, pauza, kamera)
```

---

## 9. TODO — stan projektu

Lista kontrolna przed oddaniem. Mapę pomijasz — zajmiesz się nią sam.

### Gotowe

- [x] Mag — ruch WASD, celowanie myszą, dash z i-frames
- [x] HP + mana + regeneracja many
- [x] System broni (ScriptableObject): kula ognia, 3 kostury, miecz lodowy
- [x] Sloty ekwipunku (3), podnoszenie `E`, przełączanie `1/2/3`
- [x] HUD — paski HP/many, ring dasha, pasek broni, panel „i” z opisem LPM/PPM
- [x] Menu główne (`Menu.unity`) + scena walki w Build Settings
- [x] Pauza ESC (`PauseMenu`) — działa
- [x] Game Over + Wygrana (`GameOverMenu`, `WinOnDestroy` na bossie)
- [x] Gracz startuje z zieloną kulą ognia (`Player_Mage` → `startingWeapon`)
- [x] Pickupy na scenie: Kostur 1, Kostur 2, miecz lodowy
- [x] Boss ze starym `BossController` + warunek wygranej
- [x] Wrogowie na scenie (stary `EnemyController`) + 1× `EnemyNewFinal` (nowe AI Hexfire)
- [x] Kod nowego AI: `HexfireEnemyController` (7 trybów ruchu, 11 wzorców ataku)
- [x] Kostur 3 — serpentine LPM + Verdant Nova PPM (asset + prefab pickup)
- [x] Kolorowe pociski kosturów (pomarańcz / niebieski / zielony — odróżnione od `EnemyBullet`)
- [x] README — opis mechaniki, broni, AI

### Do zrobienia (Ty)

- [ ] **Mapa / arena** — układ pokoi, rozmieszczenie wrogów i pickupów *(robisz sam)*
- [ ] **Pickup Kostur 3** — `Staff03.prefab` nie jest jeszcze na `SampleScene` (trzeba położyć na mapie)
- [ ] **Więcej wrogów Hexfire** — opcjonalnie zamiana starych `EnemyController` na prefaby z `HexfireEnemyController` (Ghost, Mimic itd.)
- [ ] **Boss Hexfire** — opcjonalnie podmiana `BossController` → `HexfireBossController`
- [ ] **Pauza** — dopracowanie wyglądu (spójność z menu głównym)
- [ ] **Build Windows** — `.exe` + krótka instrukcja w §6
- [ ] **Screenshoty** — §7 (walka, HUD, menu, boss)
- [ ] **Linki Asset Store** — §4 (konkretne URLe pakietów)
- [ ] **Git** — commit / push repozytorium (jeśli wymagane na zaliczenie)
- [ ] **Bibliografia** — §8 (opcjonalnie)

### Szybki test przed oddaniem

1. `Menu.unity` → Start → gra ładuje `SampleScene`
2. Walka: LPM/PPM każdej broni, dash, śmierć → Game Over → Retry / Exit
3. Zabij bossa → panel wygranej
4. ESC → pauza → wznowienie / menu
5. Build `.exe` uruchamia się bez Unity

---

*Ostatnia aktualizacja dokumentacji: wersja prototypu po wdrożeniu systemu Hexfire (broń + HUD + mag + game over / win + kostur szmaragdowy + dokumentacja AI).*
