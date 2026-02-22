# Game of Life - Документация проекта

## Обзор архитектуры

Проект реализует симуляцию "Игра в жизнь" с модульной архитектурой
которая поддерживает несколько режимов симуляции.

### Основные компоненты

1. **Core/** - Ядро симуляции
   - SimulationEngine - главный движок
   - SimulationConfig - конфигурация
   - WorldState - состояние мира
   - ISimulationMode - интерфейс режима

2. **Modes/** - Режимы симуляции
   - ConwayMode - классическая игра в жизнь
   - EcosystemMode - экосистема с ботами

3. **Rendering/** - Система рендеринга
   - IRenderer - интерфейс рендерера
   - BitmapRenderer - отрисовка в память
   - WinFormsRenderer - отрисовка в UI
   - NullRenderer - headless режим

4. **Entities/** - Сущности мира
   - IEntity - общий интерфейс
   - Bot - автономный агент
   - Structure - структуры

5. **Resources/** - Система ресурсов
   - ResourceType - типы ресурсов
   - ResourcePool - хранилище ресурсов
   - ConversionRecipe - рецепты конвертации

6. **Genome/** - Генетическая система
   - Genome - геном бота
   - MovementInstruction - программа движения
   - BehaviorDecision - решение бота

7. **Environment/** - Окружающая среда
   - WeatherSystem - погода
   - RadiationZone - радиоактивные зоны
   - MagneticField - магнитные аномалии

8. **Utils/** - Утилиты
   - PerformanceMonitor - мониторинг производительности
   - ObjectPool - пул объектов
   - SpatialHash - пространственное хеширование

## Режимы запуска

### UI режим (по умолчанию)
```bash
GameOfLife.exe
```

### Headless режим
```bash
GameOfLife.exe --headless
```

### Экспорт в GIF
```bash
GameOfLife.exe --export
```

### Тестовый режим
```bash
GameOfLife.exe --test
```

## Конфигурация

Основные параметры в SimulationConfig:
- WorldWidth/Height - размеры мира
- TickDelayMs - задержка между тиками
- InitialDensity - плотность заполнения
- WrapAround - зацикливание границ
- EnableParallelProcessing - параллелизм
- Seed - сид для воспроизводимости

## Производительность

Для мониторинга производительности:
```csharp
engine.EnablePerformanceMonitoring = true;
string report = engine.GetPerformanceReport();
```

## Расширение

Для добавления нового режима:
1. Создайте класс реализующий ISimulationMode
2. Реализуйте методы Initialize, Step, ApplyRules
3. Зарегистрируйте режим в движке через SetMode()

## Лицензия

Проект распространяется под лицензией MIT.
```

---
Этап 13: Расширение экосистемы (новые типы ботов, мутации, эволюция)
Этап 14: Экспорт и импорт (сохранение в различные форматы, обмен мирами)