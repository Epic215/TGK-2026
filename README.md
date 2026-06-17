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
- **System broni oparty na typach** (kula, kostury, miecz) z osobnymi umiejętnościami LPM/PPM — np. spread, chaos, leczenie, odnowa many, magiczna tarcza.
- Nowy kod gry w folderze `Assets/Hexfire/` (gracz, broń, UI), obok starszego prototypu w `Assets/Scripts/`.

**Stan projektu:** To **prototyp demonstracyjny**, nie pełna gra. Działa walka na jednej scenie (`SampleScene`); brakuje m.in. pełnej pętli game over z panelem retry/exit, dopracowanego pause menu oraz rozbudowy AI wrogów.

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
| Kostur 3 (szmaragdowy) | *Planowany* | *Planowany* |

Wzorce strzału (single / spread / chaos) nawiązują do wcześniejszego prototypu `PlayerInventory`.

Pociski: prefaby z kolizją, obrażenia wrogów przez tag `Enemy` / komponenty zdrowia.

### Przeciwnicy i boss
- **Wrogowie:** podstawowe AI (gonienie gracza, strzelanie — `EnemyController`, `EnemyHealth`). Na scenie kilka instancji `Enemy`.
- **Boss:** osobny kontroler z fazami i wzorami ataków (`BossController`) — **najmocniejszy element starego prototypu**; dalsze poprawki planowane.
- Rozbudowa zachowania wrogów (taktyka, lepsze unikanie pocisków) — **do zrobienia**.

### Interfejs (HUD)
- Paski **HP** i **many** + tekst wartości.
- **Ring cooldownu** dasha.
- **Pasek 3 slotów** broni z ikonami; przycisk **„i”** z opisem LPM/PPM aktywnej broni.
- Skrypty w `Assets/Hexfire/UI/` (`PlayerHudWire`, `EquipmentBarHud`, `WeaponInfoPanel`).

### Menu i pętla gry
- **Menu główne** (`Menu` scena): start gry, wyjście (`MenuManager`).
- **Pauza (ESC):** panel wstrzymania, wznowienie, powrót do menu (`PauseMenu`) — **do dopracowania wizualnie** (spójny styl z menu głównym, półprzezroczyste tło).
- **Game Over:** po śmierci gracza obecnie tylko log w konsoli — **planowany panel** z animacją, przyciskami **Retry** (przeładowanie sceny) i **Exit** (menu), w stylu menu głównego z przyciemnionym tłem.

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

Nie wykorzystano AI do: pełnej fabuły, muzyki, proceduralnej mapy ani uczenia maszynowego zachowania wrogów (AI przeciwników = klasyczne skrypty / maszyna stanów w kodzie).

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
├── UI/            — HUD, pasek broni, panel informacji
└── Editor/        — menu pomocnicze Hexfire (setup maga, animacje, materiały URP)

Assets/Scripts/    — starszy prototyp (wrogowie, boss, menu, pauza, kamera)
```

---

*Ostatnia aktualizacja dokumentacji: wersja prototypu po wdrożeniu systemu Hexfire (broń + HUD + mag).*
