<pre>
# Three Matching Game
[Unity Version 2020.1.17f1] 

[파일 소개]
1. Match Three_Play : 유니티로 빌드 된 실행파일입니다.

2. Match Three_Unity : 유니티 프로젝트 전체 내용입니다.
                   코드는 Match Three_Unity -> Assets -> Scripts 에 위치합니다.
                   (Match Three_Unity -> Backup Scripts 에도 수정 전 코드들이 남아있습니다.)

[게임 코드 간단한 소개]

1. GameTypes.cs : MatchedType (매칭 후 동작될 상태들)
                  IconType (4, 5 매칭에 대한 특수 아이콘 여부)
                  SwapResult (플레이어가 아이콘을 스왑시 결과)
                  SearchType (중복 매칭 검색 방지용)
                  
2. Main.cs : 시작 화면을 담당하고 기본적인 옵션 설정이 가능합니다.

3. PlayerController.cs : 클릭 된 아이콘이 폭탄인지, 클릭 후 드래그를 통해 스왑을 하였는지 등을
                         판별합니다.

4. PuzzleGrid.cs : 게임은 클릭이 가능한 별도의 그리드가 존재합니다. 해당 그리드 스크립트를 통해
                   클릭 된 위치를 판별합니다.

5. PuzzleManager.cs : 게임의 주요 로직이고 굉장히 길어졌습니다.. 매칭이 되어 아이콘이 사라지는 애니메이션 중에도
                      게임을 진행할 수 있도록 Update() 의 matchingQueue 를 두어 매칭과 채우기를 하고 있습니다.
                      게임의 흐름은 Swap -> CheckMatching / Matching -> CrashPuzzle -> FillPuzzle => matchingQueue
                      모두 matchingQueue 를 통해서 처리를 하고 있습니다.

6. PuzzleObject.cs : 보여지는 아이콘에 대한 정보를 담고 있습니다. 
                     colorType(색 또는 문양), iconType(폭탄/보통), isUsing (채우기에 사용 중), isSwapping (교환 중)
                     FadeOut 이 주요 애니메이션이며 단순히 사라지게 만들어놨습니다.

7. PuzzleObjectPool.cs : FadeOut 및 Fill 에 사용되는 오브젝트들을 재활용하기 위한 오브젝트 풀을 사용하고 있습니다.

8. Timer.cs : 게임의 시간초, 메시지 등을 담당합니다.
</pre>
