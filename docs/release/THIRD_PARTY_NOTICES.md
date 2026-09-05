# Mobil DWG — Third-Party Notices

Bu dosya release-facing üçüncü taraf bileşen özetidir. Exact resolved dependency/native graph için repo içindeki `compliance/DEPENDENCY_EVIDENCE.md`, package lock kayıtları ve final artifact audit'i esas alınır.

## Marka feragatnamesi

AutoCAD ve DWG, Autodesk, Inc. ile ilişkili ticari markalardır. Mobil DWG bağımsız bir projedir; Autodesk, Inc. tarafından onaylanmış, desteklenmiş veya Autodesk ile bağlantılı olduğu izlenimi verilmez.

## Ana üçüncü taraf bileşenler

### ACadSharp `3.7.1`

- Lisans: MIT
- Kullanım: read-only DWG/DXF parser adapter baseline
- Production sürümü repo package pinleriyle exact tutulur.

### SkiaSharp `4.151.1`

- Lisans: MIT package hattı; native/Skia third-party notices ayrıca artifact/compliance audit kapsamındadır.
- Kullanım: 2D rendering.

### Microsoft .NET MAUI `10.0.100`

- Lisans: MIT
- Kullanım: Android application/UI framework.

### System.Text.Encoding.CodePages `10.0.1`

- Lisans: MIT
- Kullanım: legacy Windows code-page desteği, Türkçe/CP1254 dahil ilgili text decoding yolları.

Resolved graph bu kısa listenin transitive bileşenlerinden daha geniş olabilir. Bu nedenle final release'te yalnız bu tabloya bakılarak “tüm lisanslar tamam” sonucu çıkarılmaz.

## Dependency/asset politikası

Production graph için:

- exact version/source/provenance,
- transitive dependency,
- native binary,
- font/asset,
- redistribution/notice

kontrolleri `compliance/LICENSE_POLICY.md` kurallarına tabidir.

Proprietary/trial CAD SDK, zorunlu ücretli runtime servis veya RED lisanslı dependency production graph'a alınmaz.

Proprietary Autodesk SHX/font dosyaları uygulama bundle'ına eklenmez.

## MIT lisans metni

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
