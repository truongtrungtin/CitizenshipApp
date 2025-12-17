
---

## 3) Tạo folder `docs/` + UX rules
**Path:** `CitizenshipApp/docs/UX-RULES.md`

```md
# UX Rules (Elderly-first)

## Mục tiêu
- Giảm tải nhận thức: 1 màn hình = 1 hành động chính
- Dễ đọc: chữ lớn, tương phản cao
- Dễ bấm: nút lớn, khoảng cách đủ rộng
- Lỗi phải “chỉ cách sửa”, không mơ hồ

## Typography
- Font cơ bản: **18–20px** tối thiểu
- Tiêu đề: **28–36px**
- Line height: **1.4–1.6**
- Tránh đoạn dài, ưu tiên câu ngắn + bullet

## Buttons & touch targets
- Touch target tối thiểu: **48px height**
- 1 nút “Primary” chính trên mỗi màn hình
- Action phụ cũng nên là button (đừng là link nhỏ)

## Forms
- Hạn chế nhập liệu; ưu tiên lựa chọn dạng card/radio
- Validate inline + hướng dẫn cách sửa
- Label rõ ràng, không viết tắt

## Navigation
- Luôn có **Back** trong flow nhiều bước
- Có **Help/FAQ** đơn giản, dễ tìm

## Audio (MVP)
- Nút “Nghe / Nghe lại” lớn
- Tốc độ: Chậm / Bình thường
- Không auto-play trừ khi user bật

## Error handling (must-have)
- Message thân thiện
- Có ít nhất 1 hành động gợi ý: Retry / Check network / Contact support
- Không show stack trace cho user
