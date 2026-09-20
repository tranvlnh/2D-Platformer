# Giới thiệu chung về OOP

## 1. OOP là gì?

OOP (Object-Oriented Programming - lập trình hướng đối tượng) là cách tổ chức chương trình xoay quanh **đối tượng** thay
vì chỉ tập trung vào các hàm và dữ liệu riêng lẻ.

Một đối tượng thường gồm:

- **Dữ liệu (state):** các thuộc tính mô tả đối tượng.
- **Hành vi (behavior):** các phương thức mà đối tượng có thể thực hiện.

Ví dụ trong game, một enemy có thể có vị trí, máu, tốc độ và các hành vi như di chuyển, tấn công hoặc nhận sát thương.

OOP giúp chương trình dễ mở rộng, tái sử dụng và bảo trì hơn khi dự án có nhiều đối tượng và hành vi phức tạp.

## 2. Bốn tính chất của OOP

### 2.1. Đóng gói (Encapsulation)

Đóng gói là việc gom dữ liệu và các phương thức xử lý dữ liệu đó vào cùng một class, đồng thời giới hạn quyền truy cập
trực tiếp từ bên ngoài.

Trong C#, có thể dùng `private`, `public`, `protected` và property để kiểm soát truy cập.

```csharp
public class Player
{
    private int health;

    public void TakeDamage(int damage)
    {
        health -= damage;
    }
}
```

Ở ví dụ trên, code bên ngoài không thể tùy ý thay đổi `health` mà phải thông qua phương thức `TakeDamage()`.

Lợi ích:

- Bảo vệ dữ liệu khỏi thay đổi không hợp lệ.
- Giảm sự phụ thuộc giữa các class.
- Dễ thay đổi cách triển khai bên trong mà không ảnh hưởng code sử dụng bên ngoài.

### 2.2. Trừu tượng (Abstraction)

Trừu tượng là chỉ cung cấp những thông tin cần thiết và ẩn đi các chi tiết triển khai phức tạp.

Ví dụ, người chơi chỉ cần gọi `Move()` để nhân vật di chuyển mà không cần biết bên trong phương thức xử lý vật lý, vận
tốc hay va chạm như thế nào.

Trong C#, abstraction thường được thể hiện bằng:

- `interface`.
- `abstract class`.
- Các phương thức public cung cấp chức năng cần dùng.

```csharp
public interface IDamageable
{
    void TakeDamage(int damage);
}
```

Interface trên chỉ mô tả đối tượng có khả năng nhận sát thương, không quy định chi tiết cách xử lý sát thương.

### 2.3. Kế thừa (Inheritance)

Kế thừa cho phép một class con sử dụng lại dữ liệu và hành vi của class cha, sau đó bổ sung hoặc thay đổi hành vi riêng.

```csharp
public class Enemy
{
    public void Move()
    {
        // Logic di chuyển chung
    }
}

public class PatrollingEnemy : Enemy
{
    public void Patrol()
    {
        // Logic đi tuần riêng
    }
}
```

`PatrollingEnemy` kế thừa từ `Enemy`, nên có thể sử dụng `Move()` và thêm hành vi `Patrol()`.

Lợi ích:

- Tái sử dụng code.
- Mô tả mối quan hệ cha - con giữa các loại đối tượng.
- Dễ mở rộng các loại enemy, player hoặc item mới.

Không nên lạm dụng kế thừa khi các class không thực sự có quan hệ “là một loại của”. Trong nhiều trường hợp, composition
phù hợp hơn inheritance.

### 2.4. Đa hình (Polymorphism)

Đa hình là khả năng cùng một lời gọi phương thức nhưng có thể tạo ra hành vi khác nhau tùy theo đối tượng thực tế.

```csharp
public abstract class Enemy
{
    public abstract void Attack();
}

public class MeleeEnemy : Enemy
{
    public override void Attack()
    {
        // Tấn công cận chiến
    }
}

public class RangedEnemy : Enemy
{
    public override void Attack()
    {
        // Tấn công từ xa
    }
}
```

Code có thể làm việc với kiểu `Enemy`, nhưng khi gọi `Attack()`, mỗi class con sẽ thực hiện cách tấn công riêng.

Đa hình thường được thực hiện thông qua:

- `virtual` và `override`.
- `abstract` và `override`.
- `interface`.

## 3. Bản chất của class

Class là một **khuôn mẫu (blueprint)** hoặc một **kiểu dữ liệu do lập trình viên định nghĩa**. Class mô tả một đối tượng
sẽ có dữ liệu nào và thực hiện được những hành vi nào.

```csharp
public class Player
{
    public string playerName;
    public int health;

    public void Jump()
    {
        // Logic nhảy
    }
}
```

Bản thân class thường chỉ là định nghĩa. Khi khai báo class, ta chưa tạo ra một player cụ thể trong bộ nhớ. Class định
nghĩa:

- **Field:** biến lưu dữ liệu.
- **Property:** cách truy cập dữ liệu có kiểm soát.
- **Method:** hành vi của đối tượng.
- **Constructor:** cách khởi tạo đối tượng.
- **Event:** sự kiện mà đối tượng có thể phát ra.

Có thể hiểu class giống như bản thiết kế của một ngôi nhà. Bản thiết kế mô tả ngôi nhà có phòng, cửa và cầu thang, nhưng
bản thân bản thiết kế chưa phải là một ngôi nhà cụ thể.

## 4. Bản chất của object

Object là một **instance (thể hiện)** được tạo ra từ class. Object tồn tại trong bộ nhớ và có dữ liệu riêng, dù được tạo
từ cùng một class.

```csharp
Player playerA = new Player();
Player playerB = new Player();

playerA.playerName = "Player A";
playerB.playerName = "Player B";
```

`playerA` và `playerB` đều có cấu trúc của class `Player`, nhưng là hai object khác nhau và có trạng thái riêng.

Một object thường được mô tả bởi ba yếu tố:

1. **Identity:** danh tính riêng, phân biệt object này với object khác.
2. **State:** trạng thái hiện tại, chẳng hạn vị trí, máu hoặc tốc độ.
3. **Behavior:** hành vi mà object có thể thực hiện thông qua các method.

Trong Unity, một GameObject có thể xem là một đối tượng trong scene. Các component như `Transform`, `Rigidbody2D`,
`Animator` hoặc script `PlayerController` cung cấp dữ liệu và hành vi cho GameObject đó.

## 5. Mối quan hệ giữa class và object

```text
Class  --tạo ra-->  Object
```

- **Class:** định nghĩa object có gì và làm được gì.
- **Object:** thể hiện cụ thể của class khi chương trình đang chạy.
- Một class có thể tạo ra nhiều object.
- Mỗi object có thể có state khác nhau.

Ví dụ, `Player` là class; nhân vật người chơi thật trong scene là một object được tạo từ class đó.

## 6. Tóm tắt

OOP tổ chức chương trình bằng các class và object. Bốn tính chất chính gồm:

| Tính chất  | Ý nghĩa                                           |
|------------|---------------------------------------------------|
| Đóng gói   | Bảo vệ dữ liệu và kiểm soát cách truy cập         |
| Trừu tượng | Ẩn chi tiết phức tạp, chỉ expose phần cần thiết   |
| Kế thừa    | Tái sử dụng và mở rộng class có sẵn               |
| Đa hình    | Cùng một interface nhưng có nhiều cách triển khai |

Nắm rõ class và object là nền tảng để xây dựng các hệ thống trong Unity như player controller, enemy, item, state
machine và component.
