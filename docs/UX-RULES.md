# UX Rules (Elderly-first)

Mục tiêu của app: người lớn tuổi có thể học đều mỗi ngày với thao tác tối thiểu, chữ lớn, tương phản cao, và lỗi dễ hiểu.

## Nguyên tắc cốt lõi
- **SystemLanguage (UI)** tách riêng với **Language (nội dung câu hỏi)** để người học có thể dùng giao diện tiếng Việt nhưng vẫn luyện câu hỏi tiếng Anh.
- **Giảm tải nhận thức**: 1 màn hình = 1 hành động chính.
- **Đọc dễ**: chữ lớn, tương phản cao, ít chữ.
- **Bấm dễ**: nút lớn, khoảng cách thoáng.
- **Sai phải biết sửa**: lỗi phải nói rõ “vì sao” và “làm gì tiếp theo”.

## Typography
- Font cơ bản: tối thiểu **18–20px**.
- Tiêu đề: **28–36px**.
- Line height: **1.4–1.6**.
- Ưu tiên câu ngắn + bullet; tránh đoạn dài.
- Không dùng chữ viết tắt nếu không cần.

## Buttons & touch targets
- Touch target tối thiểu: **48px** chiều cao.
- Mỗi màn hình nên có **1 nút Primary** rõ ràng.
- Action phụ cũng dùng button (tránh link nhỏ khó bấm).
- Trạng thái rõ ràng: default / hover / disabled / loading.

## Forms
- Hạn chế nhập liệu; ưu tiên lựa chọn dạng card / radio / dropdown.
- Validate inline và chỉ rõ cách sửa.
- Label rõ ràng, đặt gần input.
- Không bắt người dùng nhớ dữ liệu giữa các bước.

## Navigation
- Luôn có **Back** trong flow nhiều bước.
- Không ẩn đường quay lại (đặc biệt trên mobile).
- Có **Help/FAQ** đơn giản, dễ tìm.

## Study screen
- 1 câu hỏi trên 1 màn hình.
- Option trả lời là các nút lớn.
- Feedback sau khi trả lời: đúng/sai + đáp án đúng.
- Tiến độ hôm nay: hiển thị đơn giản (đã trả lời, đúng, mục tiêu).

## Audio (MVP)

- Nút “Nghe / Nghe lại” lớn.
- Có nút “Dừng/Stop” để ngắt đọc ngay.
- Tốc độ: Chậm / Vừa / Nhanh (theo `AudioSpeed`).
- Có **SilentMode**: tắt toàn bộ audio trong UI (chỉ học dạng chọn đáp án).
- Không auto-play trừ khi user bật.

## Error handling (must-have)
- Message thân thiện và cụ thể.
- Có ít nhất 1 hành động gợi ý: Retry / Check network / Contact support.
- Không show stack trace cho user.
