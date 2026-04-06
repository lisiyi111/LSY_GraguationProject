import re
import sys
import zipfile
from pathlib import Path


def main() -> int:
    if len(sys.argv) < 2:
        print("Usage: python extract_docx_text.py <docx_path>")
        return 1

    docx_path = Path(sys.argv[1])
    if not docx_path.exists():
        print(f"File not found: {docx_path}")
        return 2

    with zipfile.ZipFile(docx_path) as zf:
        xml = zf.read("word/document.xml").decode("utf-8", errors="ignore")

    parts = re.findall(r"<w:t[^>]*>(.*?)</w:t>", xml)
    text = "\n".join(parts)
    text = (
        text.replace("&amp;", "&")
        .replace("&lt;", "<")
        .replace("&gt;", ">")
        .replace("&quot;", '"')
    )
    print(text)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
