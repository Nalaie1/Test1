# Unit Tests và Benchmark Tests

Project này chứa unit tests và benchmark tests để so sánh hiệu suất giữa các phương thức đệ quy và không đệ quy để load comment tree.

## Cấu trúc

- **TestDataGenerator**: Tạo dữ liệu test với cấu trúc comment tree phân cấp
- **CommentRepositoryTests**: Unit tests để verify các phương thức hoạt động đúng
- **CommentRepositoryBenchmark**: Benchmark tests để so sánh hiệu suất

## Các phương thức được so sánh

1. **GetAllCommentsRecursive**: Sử dụng SQL CTE recursive (đệ quy ở database level)
2. **GetAllCommentsIterative**: Load tất cả comments một lần, sau đó build tree bằng iterative approach (không đệ quy)
3. **GetAllCommentsForPost**: Sử dụng EF Core Include (chỉ load 2 levels)

## Chạy Unit Tests

```bash
dotnet test
```

## Chạy Benchmark

Có một project riêng để chạy benchmark:

```bash
cd WebApplication1.Benchmark
dotnet run -c Release
```

Hoặc chạy trực tiếp từ root:

```bash
dotnet run --project WebApplication1.Benchmark -c Release
```

## Kết quả Benchmark

Benchmark sẽ so sánh:
- **Memory allocation**: Bộ nhớ được sử dụng
- **Execution time**: Thời gian thực thi
- **Throughput**: Số lượng operations/giây

Với các tham số khác nhau:
- TopLevelCount: 10, 50, 100 (số lượng comment cấp 1)
- MaxDepth: 3, 5 (độ sâu tối đa)
- RepliesPerComment: 2, 4 (số replies mỗi comment)

## Test Data Generator

`TestDataGenerator.GenerateCommentTree()` tạo comment tree với:
- Số lượng top-level comments có thể cấu hình
- Độ sâu tối đa có thể cấu hình
- Số lượng replies mỗi comment có thể cấu hình

Ví dụ: Với topLevelCount=10, maxDepth=5, repliesPerComment=3, sẽ tạo ra một tree rất lớn với nhiều levels.

