# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**6AxisTester** — 카메라 모듈(OIS/AF 액추에이터) 6축 특성 검사 장비의 제어 소프트웨어.
- Namespace: `FZ4P`
- Assembly: `6AxisTester.exe`
- Target: .NET Framework 4.7.2, WinForms, C# 7.3

## Build

Visual Studio에서 `FZ4P.csproj`를 열어 빌드한다.
- Debug 빌드: `bin\Debug\6AxisTester.exe`
- Release 빌드(x64): `bin\Release\6AxisTester.exe`

MSBuild CLI:
```
msbuild FZ4P.csproj /p:Configuration=Debug
msbuild FZ4P.csproj /p:Configuration=Release /p:Platform=x64
```

NuGet 패키지는 `../packages/` 경로에 있으며, 외부 DLL은 `../lib/`에 위치한다.
Matrox MIL은 로컬 설치 경로(`C:\Program Files\Matrox Imaging\MIL\`)에 의존한다.

## Architecture

### 전역 상태 — `STATIC` (Process/STATICS.cs)

모든 핵심 싱글톤 객체는 `STATIC` 정적 클래스에서 관리된다:

```
STATIC.Rcp       → Recipe       (설정/스펙/레시피 데이터 - XML 파일로 영속화)
STATIC.Process   → Process      (테스트 시퀀스 실행 엔진)
STATIC.Dln       → DLN          (DLN USB I2C/GPIO 인터페이스)
STATIC.DrvIC     → AK73XX       (OIS/AF 드라이버 IC - I2C 통신)
STATIC.TcpConn   → HandlerConnection (핸들러 TCP 통신)
STATIC.State     → int          (화면 상태 enum: Manage/Main/Vision)
```

`STATIC.StateChange` 이벤트로 화면 전환을 트리거한다.

### 런타임 데이터 — `Global` (Vision/MySingleton.cs)

`Global.GetInstance()`로 접근하는 싱글톤. 채널별 에러, 시퀀스 상태, 테스트 플래그 등 런타임 동적 데이터를 보관한다.

### 데이터 영속화 — `DataIO` (Process/STATICS.cs)

XML 직렬화/역직렬화 확장 메서드:
- `obj.SerializeToXMLFile<T>(path)` — 백업(.bak) 후 원자적으로 저장
- `DataIO.DeserializeXMLFileToObject<T>(path)` — XML 파일 로드

런타임 데이터 기본 경로: `C:\6AxisTester\`

### 레이어 구조

```
UI Layer         → UI/F_Main.cs, F_Manage.cs, F_Vision.cs, F_Start.cs, F_Password.cs
Process Layer    → Process/PROCESS.cs (partial), Process/UserPROCESS.cs (partial)
Driver Layer     → DriverIc/DLN.cs (USB I2C/GPIO), DriverIc/AK73XX.cs (OIS/AF IC)
Vision Layer     → Vision/MyMatrox.cs (MIL + OpenCV 영상처리, namespace: S2System.Vision)
Algorithm Layer  → Process/AFLinCompCoef.cs, OISLinCompCoef.cs (선형 보정 계수)
Config Layer     → Process/RECIPE.cs (Recipe, Condition, Spec, Model, Option 등 데이터 클래스)
```

### Process 클래스 (partial)

`Process`는 두 파일로 분리된 partial class:
- `PROCESS.cs` — 필드 선언, 채널 관리, I2C 모니터링, 이벤트(`RunStart`, `RunEnd`)
- `UserPROCESS.cs` — 실제 테스트 시퀀스 구현 (AF/OIS PID 설정, Hall 캘리브레이션 등)

테스트 항목은 `ActItems` 리스트로 관리되며, 각 항목은 `FunctionPointer(int ch, string testItem, int InspCnt)` 델리게이트를 가진다.

### Vision (MyMatrox.cs / S2System.Vision)

- Matrox MIL + OpenCV (OpenCvSharp) 혼합 사용
- `MILlib` — 카메라 그랩, 이미지 처리, FAutoLearn(자동 학습 DLL) 연동
- 별도 namespace `S2System.Vision`을 사용하여 FZ4P 네임스페이스와 분리

### TCP 통신 (Process/TCPComm.cs)

핸들러(외부 장비) 연동용 TCP 클라이언트:
- `SocketInterface` 추상 클래스 → `HandlerConnection` 구현
- `STATIC.TcpConn`으로 접근, `Model.MCType == "Handler"` 일 때 활성화

## 주요 파일 경로

| 역할 | 경로 |
|------|------|
| 진입점 | `Program.cs` |
| 전역 싱글톤/유틸 | `Process/STATICS.cs` |
| 레시피/설정 데이터 클래스 | `Process/RECIPE.cs` |
| 테스트 시퀀스 | `Process/PROCESS.cs` + `UserPROCESS.cs` |
| DLN I2C 드라이버 | `DriverIc/DLN.cs` |
| AK73XX OIS/AF IC | `DriverIc/AK73XX.cs` |
| 영상처리 | `Vision/MyMatrox.cs` |
| 메인 폼 | `UI/F_Main.cs` |
| 관리자 설정 폼 | `UI/F_Manage.cs` |

## 외부 의존성 (lib/)

| 라이브러리 | 용도 |
|-----------|------|
| `dln.net.dll` | DLN USB 어댑터 (I2C Master, GPIO) |
| `Basler.Pylon.dll` | Basler 카메라 |
| `Matrox.MatroxImagingLibrary.dll` | Matrox MIL 영상처리 |
| `FAutoLearn.dll` | 자동 학습 알고리즘 (내부 DLL) |
| `alglibnet2.dll` | 수치 최적화 (alglib) |
| `OpenCvSharp4` | OpenCV .NET 바인딩 |
| `MathNet.Numerics` | 수치 연산 |

## 코드 규칙

- `STATIC.*`를 통해 싱글톤에 접근한다 (직접 `new` 하지 않음)
- 각 클래스는 `STATIC.*`를 프로퍼티로 래핑하여 짧은 이름으로 접근: `public DLN Dln { get { return STATIC.Dln; } }`
- 데이터 저장 시 `DataIO.SerializeToXMLFile<T>()` 사용 (직접 파일 쓰기 지양)
- 화면 전환은 `STATIC.State` 값 변경으로 이벤트 기반으로 처리
- 단일 인스턴스 보장: `Mutex("FZ_Test")`로 중복 실행 방지

## 설계 원칙

새로운 로직을 추가하거나 기존 코드를 수정할 때는 **SOLID 원칙**을 기본으로 하고, 상황에 맞는 **GoF 디자인 패턴**을 적극 활용한다.

### SOLID 원칙 적용 기준

- **SRP**: 클래스 하나에 책임 하나. 파일 I/O, 하드웨어 통신, UI 업데이트, 비즈니스 로직을 한 클래스에 혼합하지 않는다.
- **OCP**: 기존 코드를 수정하지 않고 확장할 수 있도록 인터페이스/추상 클래스를 활용한다. `if (type == "A")` 분기 추가 대신 새 구현체를 추가한다.
- **LSP**: 인터페이스/추상 클래스의 계약을 구현체가 완전히 이행한다. 빈 구현(`{ }`)으로 메서드를 방치하지 않는다.
- **ISP**: 클라이언트가 사용하지 않는 메서드를 강제하지 않는다. 거대한 인터페이스 대신 목적별로 작게 분리한다.
- **DIP**: 구체 클래스 대신 인터페이스에 의존한다. 하위 레이어(Driver, Vision)가 상위 레이어(Process, UI)를 직접 참조하지 않는다.

### GoF 패턴 활용 지침

| 상황 | 권장 패턴 |
|------|-----------|
| 알고리즘/동작을 런타임에 교체해야 할 때 | **Strategy** |
| 객체 생성 방식을 추상화해야 할 때 | **Factory Method / Abstract Factory** |
| 일련의 명령을 캡슐화하고 재시도/순서 관리가 필요할 때 | **Command** |
| 상태에 따라 객체 동작이 바뀔 때 | **State** |
| 이미 사용 중인 Observer(event) 패턴을 계속 활용 | **Observer** |
| 복잡한 서브시스템을 단순화할 때 | **Facade** |
| 전역 싱글톤(`STATIC.*`)에 새 객체를 추가할 때 | **Singleton** (단, 인터페이스로 노출) |
| 기존 인터페이스와 호환되지 않는 클래스를 연결할 때 | **Adapter** |
