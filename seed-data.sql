INSERT INTO Categories (Id, Name)
VALUES
    (1, 'Notebooks'),
    (2, 'Monitores'),
    (3, 'Mouses'),
    (4, 'Teclados'),
    (5, 'Headsets'),
    (6, 'Smartphones');


INSERT INTO Products
    (Id, Name, Description, CategoryId, Price, Stock, CreatedAt)
VALUES
    (1, 'Notebook Lenovo ThinkPad E14',
     'Notebook empresarial com processador Intel Core i5, 16GB de RAM e SSD de 512GB.',
     1, 4299.90, 12, '2026-09-01T10:15:00Z'),

    (2, 'Notebook Dell Inspiron 15',
     'Notebook para trabalho e estudos com Intel Core i5, 8GB de RAM e SSD de 512GB.',
     1, 3899.90, 18, '2026-09-02T11:30:00Z'),

    (3, 'Notebook Acer Aspire 5',
     'Notebook versátil com processador AMD Ryzen 7, 16GB de RAM e SSD de 512GB.',
     1, 3599.90, 7, '2026-09-03T09:45:00Z'),

    (4, 'Notebook ASUS Vivobook 15',
     'Notebook compacto para produtividade com Intel Core i7, 16GB de RAM e SSD de 1TB.',
     1, 4799.90, 5, '2026-09-04T14:20:00Z'),

    (5, 'MacBook Air M3',
     'Notebook Apple com chip M3, 16GB de memória e SSD de 512GB.',
     1, 8999.90, 4, '2026-09-05T16:00:00Z'),

    (6, 'Monitor LG UltraGear 27',
     'Monitor gamer de 27 polegadas com resolução QHD, painel IPS e alta taxa de atualização.',
     2, 2199.90, 15, '2026-09-06T10:00:00Z'),

    (7, 'Monitor Samsung Odyssey G5',
     'Monitor gamer curvo de 27 polegadas com resolução QHD e taxa de atualização de 165Hz.',
     2, 2399.90, 9, '2026-09-07T13:15:00Z'),

    (8, 'Monitor Dell UltraSharp 27',
     'Monitor profissional de 27 polegadas com painel IPS, resolução QHD e excelente reprodução de cores.',
     2, 3299.90, 6, '2026-09-08T15:40:00Z'),

    (9, 'Monitor AOC Hero 24',
     'Monitor gamer de 24 polegadas com resolução Full HD e taxa de atualização de 144Hz.',
     2, 1199.90, 21, '2026-09-09T09:20:00Z'),

    (10, 'Monitor LG 24MP400',
     'Monitor Full HD de 24 polegadas para trabalho, estudos e uso cotidiano.',
     2, 899.90, 30, '2026-09-10T11:10:00Z'),

    (11, 'Mouse Logitech MX Master 3S',
     'Mouse sem fio ergonômico para produtividade com sensor de alta precisão e conexão Bluetooth.',
     3, 649.90, 14, '2026-09-11T10:30:00Z'),

    (12, 'Mouse Logitech G502 Hero',
     'Mouse gamer com sensor de alta precisão, iluminação RGB e botões programáveis.',
     3, 399.90, 20, '2026-09-12T12:00:00Z'),

    (13, 'Mouse Razer DeathAdder V3',
     'Mouse gamer leve com sensor óptico de alta precisão e design ergonômico.',
     3, 499.90, 8, '2026-09-13T14:45:00Z'),

    (14, 'Mouse Microsoft Bluetooth Ergonomic',
     'Mouse ergonômico sem fio com conexão Bluetooth e design confortável para longas jornadas.',
     3, 299.90, 11, '2026-09-14T16:20:00Z'),

    (15, 'Mouse Redragon Cobra',
     'Mouse gamer com iluminação RGB, sensor óptico e sete botões programáveis.',
     3, 159.90, 35, '2026-09-15T09:00:00Z'),

    (16, 'Teclado Mecânico Redragon Kumara',
     'Teclado mecânico compacto com switches Outemu e iluminação RGB.',
     4, 249.90, 25, '2026-09-16T10:10:00Z'),

    (17, 'Teclado Logitech MX Keys',
     'Teclado sem fio de perfil baixo para produtividade, com conexão Bluetooth e digitação silenciosa.',
     4, 699.90, 10, '2026-09-17T13:30:00Z'),

    (18, 'Teclado Razer BlackWidow V4',
     'Teclado mecânico gamer com switches mecânicos, iluminação RGB e teclas programáveis.',
     4, 999.90, 6, '2026-09-18T15:00:00Z'),

    (19, 'Teclado HyperX Alloy Origins',
     'Teclado mecânico gamer com estrutura em alumínio e iluminação RGB.',
     4, 599.90, 13, '2026-09-19T11:45:00Z'),

    (20, 'Teclado Microsoft Designer Compact',
     'Teclado compacto e sem fio com design minimalista para produtividade.',
     4, 449.90, 17, '2026-09-20T08:30:00Z'),

    (21, 'Headset HyperX Cloud II',
     'Headset gamer com som surround virtual, microfone removível e almofadas confortáveis.',
     5, 499.90, 16, '2026-09-20T10:00:00Z'),

    (22, 'Headset Logitech G Pro X',
     'Headset gamer com microfone avançado, som surround e conexão USB.',
     5, 899.90, 5, '2026-09-20T12:30:00Z'),

    (23, 'Headset Razer BlackShark V2',
     'Headset gamer com drivers de áudio de alta qualidade e microfone com isolamento de ruído.',
     5, 599.90, 9, '2026-09-21T09:15:00Z'),

    (24, 'Headset Sony WH-1000XM5',
     'Headset Bluetooth com cancelamento ativo de ruído e bateria de longa duração.',
     5, 2899.90, 3, '2026-09-21T14:00:00Z'),

    (25, 'Headset JBL Quantum 400',
     'Headset gamer USB com som surround, iluminação RGB e microfone removível.',
     5, 449.90, 12, '2026-09-21T16:30:00Z'),

    (26, 'Smartphone Samsung Galaxy S25',
     'Smartphone premium com tela AMOLED, processador de alto desempenho e câmera avançada.',
     6, 5999.90, 8, '2026-09-22T09:00:00Z'),

    (27, 'Smartphone iPhone 16',
     'Smartphone Apple com câmera avançada, processador A18 e tela Super Retina.',
     6, 6999.90, 6, '2026-09-22T10:30:00Z'),

    (28, 'Smartphone Google Pixel 9',
     'Smartphone Android com câmera avançada, inteligência artificial e tela OLED.',
     6, 5499.90, 4, '2026-09-22T13:00:00Z'),

    (29, 'Smartphone Motorola Edge 50',
     'Smartphone com tela pOLED, câmera de alta resolução e carregamento rápido.',
     6, 2999.90, 14, '2026-09-22T15:20:00Z'),

    (30, 'Smartphone Xiaomi Redmi Note 14',
     'Smartphone intermediário com tela AMOLED, câmera de alta resolução e bateria de longa duração.',
     6, 1899.90, 22, '2026-09-23T10:00:00Z');