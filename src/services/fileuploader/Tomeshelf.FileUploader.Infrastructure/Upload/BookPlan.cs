using System.Collections.Generic;

namespace Tomeshelf.FileUploader.Infrastructure.Upload;

public sealed record BookPlan(string BundleName, string BookTitle, IReadOnlyList<BookFile> Files);