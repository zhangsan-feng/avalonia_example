package user

import (
	"avalonia_server/api/datastore"
	"avalonia_server/global"
	"github.com/gin-gonic/gin"
	"github.com/golang-jwt/jwt/v5"
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
	userUuid := params.Username

	token, err := global.GenerateJWT(&global.MyCustomClaims{
		Username:         params.Username,
		UserAvatar:       "",
		UserUuid:         userUuid,
		RegisteredClaims: jwt.RegisteredClaims{},
	})
	if err != nil {
		log.Println(err)
		return
	}

	rand.NewSource(time.Now().UnixNano())

	index := rand.Intn(len(datastore.UserAvatar))

	if datastore.AllUsers[userUuid] == nil {
		datastore.AllUsers[userUuid] = &datastore.User{
			Id:     userUuid,
			Name:   params.Username,
			Avatar: datastore.UserAvatar[index],
			Status: "在线",
			Conn:   nil,
		}
	}

	r.JSON(200, gin.H{
		"token": token,
		"uuid":  userUuid,
	})

}
