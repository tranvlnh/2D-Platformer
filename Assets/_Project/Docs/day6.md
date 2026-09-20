# Tổng quan về hệ thống Animation trong Unity

Trong Unity, animation của nhân vật thường được tổ chức theo nhiều lớp:

```text
Animation Clip
      ↓
Animator Controller ── quản lý trạng thái và chuyển trạng thái
      ↓
Animator Component ── phát Controller trên GameObject
      ↓
Blend Tree ── pha trộn nhiều Clip theo tham số
      ↓
Finite State Machine (FSM) ── mô hình hóa logic trạng thái
```

Các thành phần này phối hợp với nhau để nhân vật có thể chuyển động linh hoạt: đứng yên, chạy, nhảy, rơi, tấn công hoặc
chết.

## 1. Animator Controller

`Animator Controller` là asset dùng để điều khiển cách các animation được phát và chuyển đổi qua lại. Có thể xem đây là
“bộ não” của hệ thống animation trong Unity.

Một Animator Controller thường bao gồm:

- Các **State** đại diện cho từng trạng thái animation, ví dụ `Idle`, `Run`, `Jump`, `Fall`.
- Các **Transition** quy định điều kiện chuyển từ state này sang state khác.
- Các **Parameter** làm dữ liệu đầu vào cho transition hoặc Blend Tree.
- Các layer để tổ chức nhiều nhóm animation cùng lúc, chẳng hạn layer thân trên và thân dưới.
- Một state đặc biệt là `Entry`, dùng làm điểm bắt đầu.

### Ví dụ

Nhân vật platformer có thể có Controller như sau:

```text
Entry → Idle

Idle  -- Speed > 0.1 --> Run
Run   -- Speed <= 0.1 --> Idle
Any State -- Jump --> Jump
Jump  -- IsGrounded --> Idle
```

Animator Controller không tự tạo ra chuyển động. Nó chỉ quyết định **animation nào được phát, khi nào phát và chuyển
sang animation khác ra sao**.

## 2. Animation Clip

`Animation Clip` là một đoạn animation cụ thể. Clip lưu trữ dữ liệu thay đổi theo thời gian, ví dụ:

- Vị trí hoặc xoay của các transform.
- Sprite được hiển thị trong từng frame đối với nhân vật 2D.
- Giá trị scale, màu sắc hoặc độ trong suốt.
- Các thuộc tính khác có thể được keyframe.

Một clip thường đại diện cho một hành động hoặc chuyển động:

- `Idle`: nhân vật đứng yên.
- `Run`: nhân vật chạy.
- `Jump`: nhân vật nhảy.
- `Attack`: nhân vật tấn công.
- `Death`: nhân vật chết.

### Một số thuộc tính quan trọng

- **Length**: độ dài của clip.
- **Samples**: số frame được lấy mẫu mỗi giây.
- **Loop Time**: cho phép clip lặp lại. Thường dùng cho `Idle` hoặc `Run`.
- **Loop Pose**: giúp điểm đầu và cuối của clip nối liền mượt hơn.
- **Root Transform**: xác định cách xử lý chuyển động của root, đặc biệt quan trọng với animation 3D.

Trong game 2D dùng sprite sheet, mỗi frame của clip thường là một sprite khác nhau. Unity sẽ lần lượt hiển thị các
sprite đó theo thời gian để tạo cảm giác chuyển động.

## 3. Animator Component

`Animator` là Component được gắn lên GameObject để thực thi một Animator Controller.

Nếu `Animator Controller` là bản thiết kế, thì `Animator Component` là thành phần trực tiếp chạy bản thiết kế đó trên
nhân vật.

### Các thành phần thường dùng

- **Controller**: tham chiếu đến Animator Controller.
- **Avatar**: thường dùng trong animation 3D để ánh xạ xương; với nhiều dự án 2D có thể không cần.
- **Apply Root Motion**: cho phép animation điều khiển chuyển động của GameObject. Với platformer 2D, chuyển động thường
  do script và `Rigidbody2D` xử lý nên tùy chọn này thường không phải phần chính.
- **Update Mode**: xác định thời điểm Animator được cập nhật, chẳng hạn theo `Normal`, `Fixed` hoặc `Unscaled Time`.

Script có thể điều khiển Animator bằng cách cập nhật Parameter:

```csharp
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D body;

    private void Update()
    {
        animator.SetFloat("Speed", Mathf.Abs(body.linearVelocity.x));
        animator.SetBool("IsGrounded", IsGrounded());
    }

    private bool IsGrounded()
    {
        // Thay bằng logic kiểm tra mặt đất của nhân vật.
        return false;
    }
}
```

Các phương thức thường dùng:

- `SetBool`: cập nhật tham số kiểu `bool`.
- `SetFloat`: cập nhật tham số kiểu `float`.
- `SetInteger`: cập nhật tham số kiểu `int`.
- `SetTrigger`: kích hoạt một sự kiện chỉ xảy ra một lần, ví dụ bắt đầu tấn công hoặc nhảy.
- `Play`: phát trực tiếp một state theo tên.
- `CrossFade`: chuyển sang state khác với hiệu ứng hòa trộn trong một khoảng thời gian.

Nên truyền dữ liệu từ gameplay sang Animator thông qua Parameter thay vì để Animator tự quyết định logic vật lý. Script
nên quản lý việc nhân vật có đang chạm đất, vận tốc bao nhiêu hoặc có được phép tấn công hay không; Animator chủ yếu
chịu trách nhiệm hiển thị trạng thái tương ứng.

## 4. Blend Tree

`Blend Tree` là một loại state đặc biệt trong Animator Controller, dùng để pha trộn nhiều Animation Clip dựa trên một
hoặc nhiều Parameter.

Thay vì chuyển đột ngột giữa nhiều clip, Blend Tree có thể tạo ra chuyển tiếp mượt hơn.

### Ví dụ với tốc độ di chuyển

Giả sử có các clip:

```text
Speed = 0.0  → Idle
Speed = 0.5  → Walk
Speed = 1.0  → Run
```

Khi `Speed` nằm giữa `0.5` và `1.0`, Unity sẽ pha trộn `Walk` và `Run`. Cách này phù hợp khi nhân vật có nhiều mức tốc
độ hoặc khi cần chuyển động tự nhiên hơn.

### Các dạng Blend Tree phổ biến

- **1D Blend Tree**: pha trộn theo một Parameter, ví dụ `Speed`.
- **2D Simple Directional**: pha trộn theo hướng di chuyển, thường dùng với `MoveX` và `MoveY`.
- **2D Freeform Directional**: phù hợp khi có nhiều animation theo hướng và tốc độ khác nhau.
- **Direct Blend Tree**: mỗi clip có một trọng số được điều khiển trực tiếp.

Trong game 2D, Blend Tree thường được dùng cho:

- Chuyển đổi giữa đứng, đi bộ và chạy.
- Animation di chuyển theo nhiều hướng.
- Nhân vật 8 hướng trong game top-down.
- Pha trộn animation khi thay đổi vận tốc.

Với platformer 2D đơn giản chỉ có `Idle` và `Run`, transition thông thường có thể dễ hiểu hơn Blend Tree. Blend Tree hữu
ích hơn khi có nhiều mức tốc độ hoặc nhiều hướng chuyển động.

## 5. Finite State Machine (FSM)

`Finite State Machine` — máy trạng thái hữu hạn — là mô hình mô tả một đối tượng có một số lượng trạng thái hữu hạn và
các quy tắc chuyển đổi giữa những trạng thái đó.

Một FSM gồm:

- **State**: trạng thái hiện tại, ví dụ `Idle`, `Run`, `Jump`.
- **Transition**: điều kiện chuyển trạng thái.
- **Input hoặc Condition**: dữ liệu làm thay đổi trạng thái, ví dụ phím bấm, vận tốc hoặc va chạm.
- **Initial State**: trạng thái bắt đầu.

### Ví dụ FSM cho nhân vật platformer

```text
                 ┌──────────────┐
                 │              │
                 ▼              │
              ┌──────┐       ┌─────┐
        ┌────▶│ Idle │──────▶│ Run │
        │     └──────┘       └─────┘
        │        │              │
        │        │ Jump         │ Jump
        │        ▼              ▼
        │     ┌──────┐       ┌──────┐
        └─────│ Jump │◀──────│ Fall │
              └──────┘       └──────┘
                   │             │
                   └── chạm đất ┘
```

FSM có thể được triển khai ở hai nơi:

1. **FSM trong Animator Controller**: quản lý việc chuyển đổi giữa các animation.
2. **FSM trong gameplay code**: quản lý hành vi và luật chơi của nhân vật.

Hai FSM này có liên quan nhưng không nhất thiết phải giống hệt nhau. Ví dụ, gameplay có thể đang ở trạng thái
`TakingDamage`, trong khi Animator vẫn phát một clip `Hurt`. Hoặc gameplay quyết định nhân vật đang `Moving`, còn
Animator chọn giữa `Walk` và `Run` dựa trên tốc độ.

## Mối quan hệ giữa các khái niệm

| Thành phần          | Vai trò chính                                            |
|---------------------|----------------------------------------------------------|
| Animation Clip      | Lưu một chuyển động hoặc hành động cụ thể                |
| Animator Controller | Tổ chức clip thành state và định nghĩa cách chuyển state |
| Animator Component  | Chạy Controller trên GameObject                          |
| Blend Tree          | Pha trộn nhiều clip theo parameter                       |
| FSM                 | Mô hình hóa các state và transition của một đối tượng    |

Luồng hoạt động thường là:

```text
Input / Physics / Gameplay Script
              ↓
       Animator Parameters
              ↓
     Animator Controller / FSM
              ↓
        State hoặc Blend Tree
              ↓
       Animation Clip được phát
```

## Ví dụ thiết kế cho nhân vật 2D

Một cấu trúc cơ bản có thể gồm:

### Animator Parameters

```text
Speed         : Float
VerticalSpeed : Float
IsGrounded    : Bool
Jump          : Trigger
Attack        : Trigger
```

### Các state

- `Idle`: đứng yên và lặp lại.
- `Locomotion`: có thể chứa Blend Tree cho `Idle`, `Walk`, `Run`.
- `Jump`: phát một lần khi bắt đầu nhảy.
- `Fall`: phát khi nhân vật đang rơi.
- `Attack`: phát khi nhận trigger `Attack`.
- `Hurt`: phát khi nhân vật nhận sát thương.
- `Death`: phát khi nhân vật hết máu.

### Một số transition

- `Idle → Locomotion` khi `Speed > 0.1`.
- `Locomotion → Idle` khi `Speed <= 0.1`.
- `Any State → Jump` khi `Jump` được kích hoạt.
- `Jump → Fall` khi `VerticalSpeed < 0`.
- `Fall → Idle` khi `IsGrounded == true`.
- `Any State → Attack` khi `Attack` được kích hoạt.
- `Any State → Death` khi nhân vật chết.

## Animator và gameplay nên phân chia trách nhiệm như thế nào?

Một nguyên tắc dễ áp dụng là:

- **Gameplay code** quyết định nhân vật được phép làm gì và trạng thái vật lý hiện tại là gì.
- **Animator** biểu diễn trạng thái đó bằng animation phù hợp.
- **Animation Event** chỉ nên dùng cho các sự kiện gắn chặt với thời điểm trong clip, ví dụ gây sát thương tại đúng
  frame của cú đánh hoặc phát âm thanh bước chân.

Không nên dùng animation làm nguồn duy nhất để quyết định các luật quan trọng như vị trí va chạm, tốc độ di chuyển hoặc
điều kiện sống/chết. Những dữ liệu này nên được quản lý bởi gameplay code và hệ thống vật lý để tránh sai lệch giữa hình
ảnh và logic.

## Lưu ý khi xây dựng hệ thống animation

- Đặt tên state, clip và parameter nhất quán, ví dụ dùng `IsGrounded` thay vì lúc thì `Grounded`, lúc thì `OnGround`.
- Hạn chế dùng quá nhiều transition từ `Any State`, vì chúng có thể tạo ra các chuyển đổi khó dự đoán.
- Kiểm tra `Has Exit Time`: bật khi muốn clip chạy đến một thời điểm nhất định trước khi chuyển; tắt khi cần phản hồi
  ngay lập tức.
- Đặt thời gian `Transition Duration` phù hợp. Thời gian quá dài làm nhân vật phản hồi chậm, quá ngắn làm chuyển động
  giật.
- Với clip đứng yên hoặc chạy, bật lặp; với clip nhảy, tấn công hoặc chết, thường không bật lặp.
- Tách các layer khi cần phát nhiều animation đồng thời, chẳng hạn thân dưới chạy trong khi thân trên tấn công.
- Kiểm tra tên Parameter trong code vì sai chính tả thường khiến Animator không chuyển state như mong muốn.

## Kết luận

`Animation Clip` là dữ liệu chuyển động, `Animator Controller` tổ chức dữ liệu đó thành các state, còn
`Animator Component` thực thi Controller trên nhân vật. `Blend Tree` giúp pha trộn nhiều clip mượt mà theo parameter.
`Finite State Machine` cung cấp cách tư duy và mô hình hóa các trạng thái cũng như chuyển đổi của nhân vật.

Khi xây dựng game 2D platformer, nên để gameplay code quản lý vật lý và luật chơi, sau đó truyền các thông tin cần thiết
vào Animator thông qua Parameter. Cách phân chia này giúp hệ thống animation dễ mở rộng, dễ gỡ lỗi và ít bị phụ thuộc
vào từng clip cụ thể.
