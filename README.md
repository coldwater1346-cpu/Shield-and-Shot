# Shield & Shot

방패로 적의 공격을 막고, 무기의 투사체를 발사해 반격하는  
하이퍼 캐주얼 물리 액션 디펜스 게임입니다.

> 실제 개발 과정의 버전 관리는 Unity Version Control을 사용했습니다.  
> 본 저장소는 포트폴리오 공개를 위해 소스 코드를 별도로 정리한 저장소이므로, 실제 개발 과정의 커밋 기록은 포함하지 않습니다.  
> 외부 에셋, SDK 설정 및 인증 정보는 포함하지 않아 단독 실행과 빌드를 지원하지 않습니다.

## 프로젝트 정보

- 개발 기간: 2026.06.04 ~ 2026.07.07
- 개발 인원: 6인 팀 프로젝트 (개발 5명, QA 1명)
- 개발 환경: Unity 6000.3.6f1, C#
- 주요 기술: Photon Engine, 뒤끝 SDK, Google Play Games
- 협업 및 버전 관리: Unity Version Control
- 실행 환경: Windows, Android

## 프로젝트 자료

- 프로젝트 기획서 : https://app.notion.com/p/5faaa1b05e3e82aabd5b815b93de33aa?source=copy_link
- 포트폴리오 설명 영상: https://youtu.be/hXdeEc8CAR0
- [포트폴리오 보기 (PDF)](docs/Development-Portfolio.pdf)
- [포트폴리오 원본 다운로드 (PPTX, GIF 포함)](docs/Development-Portfolio.pptx)

## 담당 개발

- 로그인 및 회원가입 시스템
- Google Play Games OAuth 연동
- 서버 데이터 로드 및 관리
- 아이템 데이터 구조 설계
- 인벤토리 및 장비 관리
- 가챠 시스템
- 서버 중심 재화 검증 및 차감 처리

## 주요 구현

### 1. 전략 패턴 기반 로그인 시스템

커스텀, 게스트, Google Play Games 로그인을 공통 인터페이스로 추상화했습니다.  
UI와 인증 로직을 분리하고, 인증 성공 후 서버 데이터와 인벤토리가 순차적으로 초기화되도록 공통 로그인 파이프라인을 구성했습니다.

- [LoginCoordinator](Assets/_Project/_Scripts/DataManagement/Login/LoginCoordinator.cs)
- [PostLoginInitializer](Assets/_Project/_Scripts/DataManagement/Login/PostLoginInitializer.cs)
- [CustomLoginStrategy](Assets/_Project/_Scripts/DataManagement/Login/LoginStrategy/CustomLoginStrategy.cs)
- [GpgsLoginStrategy](Assets/_Project/_Scripts/DataManagement/Login/LoginStrategy/GpgsLoginStrategy.cs)

### 2. Flyweight 패턴 기반 아이템 구조

이름, 등급, 아이콘, 기본 능력치처럼 공유 가능한 데이터는 ScriptableObject로 관리하고,  
GUID, 강화 수치, 장착 상태처럼 사용자마다 달라지는 값은 개별 아이템 인스턴스로 분리했습니다.

- [ItemData](Assets/_Project/_Scripts/DataManagement/InventorySystem/ItemData/ItemData.cs)
- [WeaponItemData](Assets/_Project/_Scripts/DataManagement/InventorySystem/ItemData/WeaponItemData.cs)
- [ShieldItemData](Assets/_Project/_Scripts/DataManagement/InventorySystem/ItemData/ShieldItemData.cs)

### 3. 인벤토리 및 가챠 시스템

GUID를 기준으로 개별 아이템의 강화 및 장착 상태를 관리했습니다.  
장착, 복합 정렬, 강화, 합성과 데이터 변경 이벤트 기반 UI 갱신을 구현했습니다.

가챠에서는 아이템, 등급, 투사체 속성, 고유 스킬을 순차적으로 결정하고  
카드 플립과 등급별 VFX를 통해 결과를 표시했습니다.

- [InventoryManager](Assets/_Project/_Scripts/DataManagement/InventorySystem/Inventory/InventoryManager.cs)
- [InventoryUI](Assets/_Project/_Scripts/DataManagement/InventorySystem/Inventory/InventoryUI.cs)
- [GachaController](Assets/_Project/_Scripts/DataManagement/GachaSystem/GachaController.cs)

### 4. 서버 기반 게임 데이터 관리

로컬 CSV로 관리하던 아이템, 확률, 비용 데이터를 뒤끝 Chart로 이전했습니다.  
로그인 시 서버 데이터를 불러와 런타임 데이터로 변환하여, 클라이언트를 다시 배포하지 않고도 게임 데이터를 수정할 수 있도록 구성했습니다.

- [BackendChart](Assets/_Project/_Scripts/Backend/BackendChart.cs)
- [ItemDataParsingManager](Assets/_Project/_Scripts/DataManagement/DataParsing/ItemDataParsingManager.cs)

### 5. 서버 중심 재화 처리

클라이언트는 가챠와 구매를 요청하는 역할만 담당하고, 서버에서 사용자 데이터 조회, 재화 검증, 차감 및 저장을 수행하도록 구성했습니다.  
서버가 반환한 최종 결과를 기준으로 클라이언트 UI를 갱신합니다.

## 공개 범위

본 저장소에는 프로젝트에서 직접 작성한 C# 코드만 포함합니다.

다음 자료는 라이선스 및 보안상의 이유로 공개하지 않습니다.

- Asset Store 및 제3자 에셋
- 모델, 텍스처, 음원 및 폰트
- Photon, Google Play, 광고 및 뒤끝 서비스 설정
- API 키, 프로젝트 식별자 및 키스토어
