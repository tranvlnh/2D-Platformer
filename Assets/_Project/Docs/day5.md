## 1. Delegate, Action & UnityEvent

Cả ba đều là các cơ chế cho phép truyền và gọi lại hàm (callback), giúp lập trình hướng sự kiện (Observer Pattern) và giảm sự phụ thuộc lẫn nhau giữa các script (decoupling).

### 1.1 Khái niệm cơ bản
* **`delegate` (C# thuần):** Là một kiểu dữ liệu tham chiếu (reference type) trỏ đến một hoặc nhiều hàm có cùng chữ ký (signature).
* **`Action` (System.Action):** Là delegate định nghĩa sẵn của .NET không có giá trị trả về (`void`), nhận từ 0 đến 16 tham số (VD: `Action<int, string>`). Giúp code ngắn gọn mà không cần khai báo `delegate` tùy biến.
* **`UnityEvent` (UnityEngine.Events):** Là giải pháp bao bọc (wrapper) delegate của Unity, hỗ trợ hiển thị trực quan lên **Inspector** để gán hàm qua giao diện kéo-thả (kể cả trong Editor hoặc Runtime).

---

### 1.2 Cách khai báo, đăng ký và phát (Invoke) sự kiện

#### A. C# Delegate & Action (Khuyên dùng trong Code logic)
```csharp
using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    // 1. Khai báo Action
    public static event Action<int> OnHealthChanged;

    public void TakeDamage(int damage)
    {
        // 2. Cách phát (Invoke) Event: Sử dụng '?.' để tránh NullReferenceException
        OnHealthChanged?.Invoke(damage);
    }
}

public class UIManager : MonoBehaviour
{
    // 3. Cách đăng ký (Subscribe) và hủy đăng ký (Unsubscribe)
    private void OnEnable()
    {
        PlayerHealth.OnHealthChanged += UpdateHealthUI;
    }

    private void OnDisable()
    {
        // Bắt buộc hủy đăng ký để tránh memory leak
        PlayerHealth.OnHealthChanged -= UpdateHealthUI; 
    }

    private void UpdateHealthUI(int damage)
    {
        Debug.Log($"Trừ máu trên UI: {damage}");
    }
}
```

#### B. UnityEvent (Tập trung trên Inspector & Kéo-thả)
```csharp
using UnityEngine;
using UnityEngine.Events;

public class Chest : MonoBehaviour
{
    // Hiển thị khung kéo thả hàm trên Inspector
    [SerializeField] private UnityEvent onOpenChest;

    public void Open()
    {
        // Phát sự kiện
        onOpenChest?.Invoke();
    }

    // Đăng ký bằng code (nếu cần):
    // onOpenChest.AddListener(HuuIchFunction);
    // onOpenChest.RemoveListener(HuuIchFunction);
}
```

---

## 2. Coroutine trong Unity

### 2.1 Khái niệm & Luồng (Coroutine Threading)
* **Bản chất luồng:** Coroutine **không phải là đa luồng (Multi-threading)**. Nó vẫn chạy trên **Main Thread** của Unity.
* **Cơ chế:** Coroutine là hàm có khả năng **tạm dừng (pause)** thực thi tại một thời điểm (`yield return`) và trả quyền kiểm soát lại cho Unity, sau đó tiếp tục thực hiện tiếp tại đúng vị trí đó ở các frame sau.

### 2.2 Cách sử dụng & Các câu lệnh `yield return` phổ biến
Hàm Coroutine bắt buộc trả về kiểu `IEnumerator`.

* `yield return null`: Tạm dừng và tiếp tục chạy ở **frame tiếp theo**.
* `yield return new WaitForSeconds(n)`: Tạm dừng trong **n giây** (phụ thuộc vào `Time.timeScale`).
* `yield return new WaitForSecondsRealtime(n)`: Tạm dừng theo thời gian thực (không bị ảnh hưởng khi Pause game bằng `timeScale = 0`).

```csharp
using System.Collections;
using UnityEngine;

public class CoroutineExample : MonoBehaviour
{
    private Coroutine myCoroutine;

    private void Start()
    {
        // 1. Khởi chạy Coroutine (StartCoroutine)
        myCoroutine = StartCoroutine(CountdownRoutine(3));
    }

    private IEnumerator CountdownRoutine(int seconds)
    {
        for (int i = seconds; i > 0; i--)
        {
            Debug.Log($"Đếm ngược: {i}");
            yield return new WaitForSeconds(1f); // Tạm dừng 1 giây
        }
        
        yield return null; // Chờ thêm 1 frame
        Debug.Log("Bùm!");
    }

    private void StopExample()
    {
        // 2. Dừng Coroutine (StopCoroutine)
        if (myCoroutine != null)
        {
            StopCoroutine(myCoroutine); // Dừng theo biến tham chiếu đã lưu
            myCoroutine = null;
        }

        // Hoặc: StopAllCoroutines(); // Dừng tất cả coroutine trên MonoBehaviour này
    }
}
```