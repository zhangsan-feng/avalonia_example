package api

import (
	"avalonia_server/global"
	"github.com/gin-gonic/gin"
	"github.com/golang-jwt/jwt/v5"
	"github.com/google/uuid"
	"log"
	"math/rand"
	"time"
)

type LoginParams struct {
	Username string `json:"username"`
	Password string `json:"password"`
}

func LoginApi(r *gin.Context) {
	params := &LoginParams{}
	err := r.ShouldBind(params)
	if err != nil {
		log.Println(err)
		return
	}
	user_uuid := uuid.New().String()

	token, err := global.GenerateJWT(&global.MyCustomClaims{
		Username:         params.Username,
		UserAvatar:       "",
		UserUuid:         user_uuid,
		RegisteredClaims: jwt.RegisteredClaims{},
	})
	if err != nil {
		log.Println(err)
		return
	}

	rand.Seed(time.Now().UnixNano())

	index := rand.Intn(len(UserAvatar))

	AllUsers[user_uuid] = &User{
		Id:     user_uuid,
		Name:   params.Username,
		Avatar: UserAvatar[index],
		Status: "在线",
		Conn:   nil,
	}
	r.JSON(200, gin.H{
		"token": token,
		"uuid":  user_uuid,
	})

}
