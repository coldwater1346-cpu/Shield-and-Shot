using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using BackEnd;


namespace Shield_Shot.NetworkCore
{
    public class BackendLogin
    {
        private static BackendLogin _instance = null;

        public static BackendLogin Instance
        {
            get
            {
                if(_instance ==null)
                {
                    _instance = new BackendLogin();
                }

                return _instance;
            }
        }
        public bool CustomSignUp(string id, string pw)
        {
            // 회원 가입 로직
            Debug.Log("회원 가입을 요청합니다.");

            var bro = Backend.BMember.CustomSignUp(id, pw);

            if (bro.IsSuccess())
            {
                Debug.Log("회원가입에 성공했습니다. : " + bro);
                return true; 
            }
            else
            {
                Debug.Log("회원가입에 실패했습니다. " + bro);
                return false;  
            }
        }

        public bool CustomLogin(string id, string pw)
        {
            //로그인 로직

            Debug.Log("로그인을 요청합니다.");

            var bro = Backend.BMember.CustomLogin(id, pw);

            if(bro.IsSuccess())
            {
                Debug.Log("로그인이 성공했습니다."+bro);
                return true;
            }
            else
            {
                Debug.Log("로그인이 실패했습니다."+bro) ;
                return false;
            }
        }

        public bool UpdateNickname(string nickname)
        {
            //닉네임 변경 로직 
            var bro =Backend.BMember.UpdateNickname(nickname);

            if(bro.IsSuccess())
            {
                Debug.Log("닉네임 변경에 성공했습니다." + bro);
                return true;
            }
            else
            {
                Debug.Log("닉네임 변경에 실패했습니다." + bro);
                return false;
            }
        }

     

        // 2. 뒤끝 서버 닉네임 중복 체크 함수 (나중에 쓸 것)
        public bool CheckNicknameDuplicate(string nickname)
        {
            Debug.Log($"[뒤끝] 닉네임 중복 검사 요청 : {nickname}");

            // 뒤끝 공식 가이드에서 제공하는 닉네임 중복 검사 API입니다.
            var bro = Backend.BMember.CheckNicknameDuplication(nickname);

            if (bro.IsSuccess())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Logout()
        {
            var bro = Backend.BMember.Logout();
            if(bro.IsSuccess())
            {
                Debug.Log("[BackendLogin] 로그아웃 성공");
            }
            else
            {
                Debug.LogError("[BackendLogin] 로그아웃 실패: " + bro);
            }
        }
    }
}