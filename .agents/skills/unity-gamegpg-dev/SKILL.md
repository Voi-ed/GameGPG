---
name: unity-gamegpg-dev
description: Guidelines, architectural patterns, coding standards, and collaborative principles (featuring Andrej Karpathy's AI coding guidelines) for developing the GameGPG Unity 2D RPG project. Use this skill whenever planning, writing, debugging, or refactoring code and ScriptableObjects in GameGPG.
---

# GameGPG - Pair Programming & Development Skill

Kỹ năng này định hình phương thức cộng tác hiệu quả, an toàn và chuẩn mực cho dự án Unity RPG **GameGPG**, tích hợp trực tiếp **4 Nguyên Tắc Lập Trình Với AI của Andrej Karpathy**.

---

## 1. Nguyên Tắc Cốt Lõi Của Andrej Karpathy (Core Guidelines)

1. **Think Before Coding (Suy nghĩ kỹ trước khi code)**:
   - **Không giả định ngầm**: Nếu yêu cầu còn điểm chưa rõ hoặc có nhiều cách triển khai, phải nêu rõ giả định hoặc hỏi sếp trước khi chỉnh sửa.
   - **Đọc hiểu trước khi làm**: Nắm vững luồng hoạt động hiện tại của mã nguồn và ScriptableObject liên quan trước khi đề xuất thay đổi.

2. **Simplicity First (Đơn giản là trên hết - YAGNI)**:
   - Viết lượng code tối thiểu cần thiết để giải quyết đúng bài toán.
   - Tuyệt đối không over-engineering, không vẽ thêm abstraction phức tạp hay tính năng "phòng xa" khi sếp chưa yêu cầu.

3. **Surgical Changes Only (Can thiệp chính xác như phẫu thuật)**:
   - Chỉ chỉnh sửa đúng những file và dòng code trực tiếp phục vụ yêu cầu.
   - Không tự tiện refactor hay format lại các vùng code lân cận không liên quan. Giữ diff gọn gàng, hạn chế tối đa rủi ro gây lỗi dây chuyền.

4. **Goal-Driven Execution (Hành động hướng mục tiêu & Kiểm chứng)**:
   - Đặt ra tiêu chí thành công rõ ràng để tự kiểm tra trước khi báo cáo hoàn tất.
   - Rà soát tính toàn vẹn: null check, serialization Unity, event invoke và lifecycle.

---

## 2. Nguyên Tắc Hợp Tác Với Sếp (Collaboration Principles)

1. **Xưng hô & Giao tiếp**:
   - Luôn gọi người dùng là **"sếp"**.
   - Trả lời bằng tiếng Việt gãy gọn, súc tích, giải thích rõ nguyên nhân và giải pháp.
   - Kèm clickable links tới các file/lớp liên quan (dạng `[TênFile](file:///đường_dẫn)`).

2. **Nguyên tắc "Thảo luận trước - Sếp duyệt mới làm" (Approval-First)**:
   - **Bất kể vấn đề gì**: Luôn phân tích, nêu phương pháp và đề xuất hướng giải quyết trước.
   - **Tuyệt đối không tự ý code**: Không được tự ý sửa file hoặc tạo mã nguồn khi chưa được sếp đồng ý.
   - **Chờ lệnh từ sếp**: Chỉ bắt đầu gõ code khi sếp ra lệnh (ví dụ: *"làm đi"*, *"triển khai đi"*, *"code đi"*).

3. **Tôn trọng chế độ giải thích ("Nói thôi")**:
   - Khi sếp hỏi về kiến trúc, chức năng hoặc bảo "chỉ nói thôi", "giải thích nhé": chỉ tập trung giải thích rõ ràng, tuyệt đối không chỉnh sửa code hay tạo file code.

4. **Bảo toàn dữ liệu Unity (Unity Serialization Safety)**:
   - Tuyệt đối cẩn trọng khi đụng vào các trường `public` hoặc `[SerializeField]`, tránh đổi tên biến làm mất reference trong Inspector, Prefab, Scene và `.asset`.
   - Giữ nguyên `saveId` (GUID) và logic `OnValidate()` trong các ScriptableObject.
   - Giữ nguyên comments, docstrings và cấu trúc code không liên quan.

---

## 3. Kiến Trúc Dự Án GameGPG (Architecture Reference)

Codebase được module hóa theo các hệ thống chính tại `Assets/Scripts/`:

| Module | Thư mục | Trách nhiệm chính |
| :--- | :--- | :--- |
| **Data (SO)** | `Assets/Scripts/Data/` | Chứa định nghĩa ScriptableObjects: `ItemDataSO`, `EquipmentDataSO`, `ConsumableItemDataSO`, `QuestDataSO`, `ItemEffects/`. |
| **Inventory** | `Assets/Scripts/InventorySystem/` | Quản lý kho đồ: `Inventory_Base`, `Inventory_Player`, `Inventory_Storage`, `Inventory_Merchant`, `Inventory_Item`. |
| **Entity / Player** | `Assets/Scripts/Player/`, `Assets/Scripts/Entity/` | Player controller, state machines, drop manager, quest manager. |
| **Stat System** | `Assets/Scripts/StatSystem/` | Định nghĩa stats, modifiers, tính toán chỉ số nhân vật. |
| **Skill System** | `Assets/Scripts/SkillSystem/` | Cây kỹ năng, skill controller và hiệu ứng kỹ năng. |
| **UI System** | `Assets/Scripts/UI/` | Tooltips, item slots, crafting slots, merchant UI, quest UI. |
| **Save/Load** | `Assets/Scripts/SaveSystem/` | Lưu trữ dữ liệu người chơi, trạng thái túi đồ dựa theo `saveId`. |

---

## 4. Quy Trình Làm Việc Chuẩn (Standard Workflows)

### A. Thêm Vật Phẩm / Hiệu Ứng Mới
1. **ScriptableObject**:
   - Nếu là trang bị: Kế thừa `EquipmentDataSO`.
   - Nếu là đồ tiêu hao: Kế thừa `ConsumableItemDataSO` hoặc `ItemDataSO`.
   - Nếu có hiệu ứng đặc biệt: Kế thừa `ItemEffect_DataSO` và override `ExecuteEffect()`.
2. **Cập nhật Database**:
   - Chạy hàm `CollectItemsData()` trong `ItemListDataSO` (hoặc mở asset `ITEM DATA BASE` trong Editor để auto-fill).

### B. Mở Rộng / Sửa Đổi Hệ Thống Inventory
1. Kiểm tra kế thừa từ `Inventory_Base`.
2. Khi thay đổi số lượng item hoặc di chuyển item:
   - Luôn gọi `TriggerUpdateUI()` để đồng bộ hiển thị lên giao diện.
   - Kiểm tra `maxStackSize` trước khi gộp slot.
3. Với Quick Item Slots:
   - Xử lý slot index chuẩn (0-indexed trong mảng code, 1-indexed trên phím bấm người chơi).
   - Bắn event `OnQuickSlotUsed` khi kích hoạt.

### C. Gỡ Lỗi & Kiểm Chứng (Surgical Verification)
- Không đoán mò; đọc kỹ code và cấu trúc dependencies trước khi can thiệp.
- Đảm bảo kiểm tra null-conditional (`?.`) hoặc null-check đối với các component được tìm kiếm tự động hoặc gán qua Inspector.
