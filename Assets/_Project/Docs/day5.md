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

## 3. Async/Await (`Awaitable`) trong Unity

`Awaitable` (từ Unity 2023 / Unity 6+) là giải pháp lập trình bất đồng bộ chuẩn hiện đại thay thế cho `Coroutine` (`IEnumerator`), giúp loại bỏ rác bộ nhớ (Zero-allocation), hỗ trợ trả về dữ liệu trực tiếp và bắt lỗi qua `try/catch`.

### 3.1 Khái niệm & Luồng (Threading)
* **Bản chất luồng:** Mặc định các tác vụ `async Awaitable` vẫn được điều phối và thực thi trên **Main Thread** của Unity, cho phép gọi và thao tác an toàn với các Unity API (`Transform`, `Rigidbody2D`, `Instantiate`,...).
* **Cơ chế:** Khi gặp từ khóa `await`, hàm sẽ tạm dừng và nhường quyền kiểm soát lại cho Unity. Sau khi tác vụ chờ hoàn tất, luồng sẽ quay lại đúng vị trí đó và tiếp tục thực hiện các câu lệnh tiếp theo.

---

### 3.2 Bảng quy đổi câu lệnh từ Coroutine sang Async/Await

| Mục đích | Coroutine (`IEnumerator`) | Async/Await (`Awaitable`) |
| :--- | :--- | :--- |
| **Chờ frame kế tiếp** | `yield return null;` | `await Awaitable.NextFrameAsync();` |
| **Chờ chu kỳ vật lý** | `yield return new WaitForFixedUpdate();` | `await Awaitable.FixedUpdateAsync();` |
| **Chờ theo thời gian game** | `yield return new WaitForSeconds(n);` | `await Awaitable.WaitForSecondsAsync(n);` |
| **Chờ cuối frame (hậu render)** | `yield return new WaitForEndOfFrame();` | `await Awaitable.EndOfFrameAsync();` |
| **Giá trị trả về** | Không hỗ trợ (phải dùng callback/event) | Hỗ trợ tự nhiên: `async Awaitable<T>` |

---

### 3.3 Cách sử dụng, Khởi chạy và Hủy tác vụ (`CancellationToken`)

Thay vì dùng `StopCoroutine()`, lập trình bất đồng bộ quản lý vòng đời và dừng tác vụ thông qua **`CancellationToken`**. Unity cung cấp sẵn `destroyCancellationToken` gắn liền với vòng đời của `MonoBehaviour`.

```csharp
using System;
using System.Threading;
using UnityEngine;

public class AsyncAwaitExample : MonoBehaviour
{
    private CancellationTokenSource _cts;

    private void Start()
    {
        // 1. Khởi chạy tác vụ (Tương đương StartCoroutine)
        StartCountdown();
    }

    private void StartCountdown()
    {
        // Tạo token để có thể chủ động hủy khi cần
        _cts = new CancellationTokenSource();

        // Kết hợp token nội bộ với token tự hủy của GameObject
        var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(
            _cts.Token, 
            destroyCancellationToken // Tự động hủy nếu GameObject bị Destroy
        ).Token;

        // Gọi hàm async an toàn
        _ = CountdownAsync(3, linkedToken);
    }

    // Hàm bất đồng bộ trả về Awaitable
    private async Awaitable CountdownAsync(int seconds, CancellationToken ct)
    {
        try
        {
            for (int i = seconds; i > 0; i--)
            {
                Debug.Log($"Đếm ngược: {i}");

                // Tạm dừng 1 giây (nếu bị hủy sẽ ném OperationCanceledException)
                await Awaitable.WaitForSecondsAsync(1f, cancellationToken: ct);
            }

            // Chờ thêm 1 frame
            await Awaitable.NextFrameAsync(cancellationToken: ct);
            Debug.Log("Bùm!");
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Tác vụ đã được dừng an toàn!");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Phát sinh lỗi: {ex.Message}");
        }
    }

    private void StopCountdown()
    {
        // 2. Dừng tác vụ (Tương đương StopCoroutine)
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }

    private void OnDestroy()
    {
        // Dọn dẹp tài nguyên khi script bị hủy
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
```