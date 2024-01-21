# Static Meshes, zadanie rekrutacyjne nr 1.

## Treść zadania:
Chcemy wyrenderować bardzo dużą liczbę takich samych meshy z tymi samymi materiałami.
Instancje mają być równomiernie rozłożone wewnątrz kuli. Mesh, materiały, ilość instancji, seed i
parametry kuli są ustalane w edytorze. Symulacja jest statyczna - instancje nie poruszają się. Instancje
są renderowane zarówno w Edit jak i w Play mode.

## Organiczenia i wymagania rozwiązania:
Odp:
1. Predefiniowana maksymalna liczba obiektów. Jakkolwiek łatwo można ją rozszerzyć, ograniczenie pozostaje.
2. Brak możliwości (póki co) na zmianę położenia środka sfery lub wprowadzenia większej ilości sfer.

## Co by się zmieniło, gdyby instancje mogły się poruszać (symulacja ruchu realizowana przez inny system)?
Odp: 

  Niewiele. W tradycyjnym sposobie tworzenia tak dużej ilość obiektów różnica między obiektami statycznymi a dynamicznymi jest znaczna. 
Statyczne obiekty współdzielące ten sam materiał są renderowane przy pomocy jednego draw call'a. (Static Batching)
Przy dynamicznych obiektach (tj ruszających się), w zależności od docelowej platformy należałoby użyć lub nie Dynamic Batching-u, 
który jest znacznie mniej wydajny i ma więcej ograniczeń względem Static Batchingu.

  W projekcie użyłem pakietu Entity Graphics, który wraz z SRP Batcherem tworzy bardzo małą liczbę zoptymalizowanych Draw Call-i nie zależnie od ruchu obiektów. 
Zatem wydajność może ucierpieć przede wszystkim na gruncie CPU w zależności od implementacji systemu poruszającego obiektami.

## Jak można ulepszyć rozwiązanie?
Odp: 
1. Spawn system uniezależnić od klasy po której dziedziczy czyli SystemBase. 
   Implementacja rozwiązania w taki sposób była szybsza i łatwieksza dla mnie, natomiast zdaję sobie sprawę że dałoby się to zrobić lepiej. 
2. Zrezygnować z ręcznego wypalania predefiniowanych pozycji przed użyciem systemu, a pozycje obiektów obliczać w czasie rzeczywistym. 
   Zrobiłem to w taki sposób, ponieważ chciałem aby pozycje kolejnych obiektów były przypisywane w kolejności od najbliższej centrum do najdalszej. 
   Natomiast sortowanie tak dużej listy pozycji przy każdym przerenderowaniu obiektów wydało mi się nie wskazane.
   Problem można by rozwiązać za pomocą lepszego algorytmu wyznaczającego kolejne pozycje obiektów. Algorytmu który zapemniałby równomierne rozłożenie w przestrzeni,
   poczynając od środka w kierunku kazdej z osi. Dodatkowo dzięki temu można by pozbyć się ograniczenia w postaci maksymalnej ilości obiektów.
   Niestety nie byłem w stanie w sensownym czasie wymyślić/odkryć takiego wzoru. 

## Jak inaczej można rozwiązać problem (krótki opis z zaletami i wadami)?
Odp:
1. Tradycyjnie. Tworzyć obiekty typu Monobehaviour. W zależności czy chcemy aby ilość obiektów mogła się zmieniać w runtime, tworzyć je albo od razu statyczne, albo korzystać z metody Combine Meshes.
Zaletą takiego sposobu jest jego prostota, szybkość implementacji i wieksza odporność na błąd programisty - w przypadku DOTS-ów i błędnym napisaniu jakiejś instrukcji regułą jest np crash Edytora.
Wadami są performance i utrudniona skalarność w przyszłości. Implementując rozwiązanie w taki sposób, zmiana zasady działania z statycznych obiektów na dynamiczne prawdopodobnie doprowadzi programistę do solidnego bólu głowy. 
