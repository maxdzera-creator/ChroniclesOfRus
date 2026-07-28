# Хроники Руси — прототип перемещения

Проект подготовлен для установленной Unity `6000.2.5f1` и новой Input System.

## Первый запуск

1. Откройте папку проекта в Unity Hub через **Add project from disk**.
2. Дождитесь импорта пакетов и компиляции.
3. Если сцена ещё не создана, выберите меню **Chronicles of Rus → Build Prototype Scene**.
4. Откройте `Assets/_Game/Scenes/Prototype_PlayerMovement.unity`.
5. Нажмите Play.

Управление: WASD, стрелки или левый стик геймпада. Подготовлены действия Space/B, ЛКМ/X, ПКМ/RT, Q/RB, E/A и Escape/Start для будущих механик.

## Состав сцены

- `Arena` — площадка с коллайдером;
- `Obstacle` — несколько тестовых препятствий;
- `Player` — `CharacterController`, чтение ввода, движение и адаптер анимации;
- дочерний `Visual` — временная капсула без собственного коллайдера;
- `Main Camera` — перспективная изометрическая камера с плавным слежением;
- `Directional Light`.

Скорость, ускорение, торможение, поворот, гравитация и параметры камеры доступны в Inspector.

## Animator

Компонент `PlayerAnimationController` безопасно работает без Animator. Когда появится Animator Controller, добавьте float-параметр `Speed`: `0` соответствует Idle, значение больше нуля — Movement. Методы для будущих Attack, Dodge и Hit уже предусмотрены.

## State Machine персонажа

`PlayerStateMachine` хранит состояния и обновляет только активное. Рабочие
`PlayerIdleState` и `PlayerMoveState` передают ввод в `PlayerMovement`. Заготовки
`PlayerDodgeState`, `PlayerAttackState`, `PlayerHurtState` и `PlayerDeathState`
зарегистрированы, но пока не подключены к игровым событиям.

Чтобы добавить состояние:

1. добавьте идентификатор в `PlayerStateId`;
2. создайте класс-наследник `PlayerState`;
3. зарегистрируйте его в `PlayerStateMachine.RegisterDefaultStates`;
4. вызывайте `ChangeState` из подходящего состояния или компонента игровой системы.

Состояния координируют компоненты, но не реализуют внутри себя ввод, физику,
здоровье или анимацию.
