# z instrukcji
/ transakcyjnosc operacji 
    - transactional outbox pattern
    - kazdy handler chodzi w db transaction, a razem z commitem idzie zapis do tabeli outbox
    - osobny proces/watek (do przemyslenia) odsyla do kolejek
/ jakos obsluzyc timeouty na kolejce i backendzie
    - cancellation tokeny strzelajace jezeli kolejka odpowiedzi sie usunela (front przestal)

# funkcjonlnosci
/ wysylanie kasy
/ historia
/ ATM simulator

/ przekazanie komendy dalej do nastepnej aplikacji (mailowej)
/ implementacja fail-fast
/ handshake + circuit breaker
/ konteneryzacja

/ queries nie muszą być w transakcjach
/ !!! sprawdzić jak działa QoS, czy na pewno ACK dobrze chodzi (punkt 7 instrukcji)

# ze zdrowego myslenia
- dodac szyfrowanie po webstompie (chyba wystarczy zmienic protokol i port w ts)
- zrobic dokladniejszy research na autoryzacje
    - moze udaloby sie jakos sprytnie trzymac JWT, bo i tak nie ma tu chyba mozliowsci ataku przez skrypty
/ dodanie wyswietlania i obslugi bledow przychodzacych z backendu na froncie
- dodanie odpowiednich uzytkownikow do rabbita i korzystanie z nich na froncie i backendzie
/ dodanie sprytnego mechanizmu odroczenia wykonania polecenia do momentu commita transkacji (np event w unit of work)
- wymineić listenery i publishera w backend na hosted service

