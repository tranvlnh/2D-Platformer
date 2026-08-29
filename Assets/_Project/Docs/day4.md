### 1. New Input System

Hệ thống xử lý input hiện đại dựa trên sự kiện và cấu hình `InputAction`.

```csharp
// Đọc giá trị Vector2 từ Action (ví dụ: phím WASD / Joystick)
Vector2 moveInput = moveAction.ReadValue<Vector2>();

// Bắt sự kiện nhấn nút (Button Action)
jumpAction.performed += ctx => Jump();

```

---

### 2. Rigidbody2D

Thành phần gán thuộc tính vật lý (khối lượng, vận tốc, trọng lực) cho đối tượng 2D. Luôn xử lý logic di chuyển trong
`FixedUpdate`.

* **Dynamic:** Bị ảnh hưởng bởi trọng lực và lực tác động (dùng cho nhân vật, vật rơi).
* **Kinematic:** Không chịu tác động của lực/trọng lực, di chuyển bằng code (dùng cho thang máy, bệ nâng di động).
* **Static:** Đứng yên hoàn toàn, tối ưu hiệu năng (dùng cho tường, sàn nhà).

```csharp
private Rigidbody2D rb;

void FixedUpdate()
{
    // Di chuyển trực tiếp bằng vận tốc
    rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y);
}

```

---

### 3. Physics2D & Collision vs Trigger

Xác định cách các Collider2D tương tác với nhau khi va chạm:

* **Collision (`Is Trigger = false`):** Tạo ra va chạm vật lý có lực cản (vật không thể đi xuyên qua nhau).
* **Trigger (`Is Trigger = true`):** Không cản trở di chuyển, chỉ phát hiện khi có vật đi xuyên qua (dùng cho vùng nhặt
  đồ, checkpoint, bẫy).

```csharp
// Xử lý va chạm vật lý
private void OnCollisionEnter2D(Collision2D collision)
{
    Debug.Log($"Va chạm vật lý với: {collision.gameObject.name}");
}

// Xử lý khi đi vào vùng Trigger
private void OnTriggerEnter2D(Collider2D other)
{
    Debug.Log($"Đã đi vào vùng của: {other.gameObject.name}");
}

```

---

### 4. LayerMask

Bộ lọc sử dụng Bitmask để giới hạn đối tượng mà các hàm vật lý (như Raycast, Overlap) tương tác, giúp tối ưu hiệu năng
và tránh va chạm nhầm layer.

```csharp
[SerializeField] private LayerMask groundLayer;

// Kiểm tra layer của đối tượng va chạm có nằm trong LayerMask không
if (((1 << other.gameObject.layer) & groundLayer) != 0)
{
    // Hợp lệ
}

```

---

### 5. Raycast 2D

Bắn một tia từ điểm xuất phát theo hướng xác định để kiểm tra vật cản (thường dùng để check chạm đất - *Ground Check*).

```csharp
[SerializeField] private LayerMask groundLayer;
[SerializeField] private float checkDistance = 0.2f;

bool IsGrounded()
{
    // Bắn tia từ chân nhân vật xuống dưới
    RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, checkDistance, groundLayer);
    
    // Trả về true nếu tia chạm vào Collider thuộc LayerMask
    return hit.collider != null;
}

// Vẽ trực quan tia để debug trên Scene view
void OnDrawGizmos()
{
    Gizmos.color = Color.red;
    Gizmos.DrawLine(transform.position, transform.position + Vector3.down * checkDistance);
}

```