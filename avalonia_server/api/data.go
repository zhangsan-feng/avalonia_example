package api

import (
	"github.com/google/uuid"
	"github.com/gorilla/websocket"
	"log"
	"os"
)

type User struct {
	Id            string          `json:"id"`
	Name          string          `json:"name"`
	Avatar        string          `json:"avatar"`
	Conn          *websocket.Conn `json:"-"`
	Status        string          `json:"state"`
	MessageGroups []string        `json:"message_groups"`
	FriendGroups  []string        `json:"friend_groups"`
}

type GroupHistory struct {
	MessageId      string   `json:"message_id"`
	SendGroupId    string   `json:"send_group_id"`
	SendUserId     string   `json:"send_user_id"`
	SendUserName   string   `json:"send_username"`
	SendUserAvatar string   `json:"send_user_avatar"`
	Message        string   `json:"message"`
	Time           string   `json:"time"`
	Files          []string `json:"files"`
}
type GroupMembers struct {
	Id       string `json:"id"`
	Name     string `json:"name"`
	Avatar   string `json:"avatar"`
	Usertype string `json:"user_type"`
	Status   string `json:"status"`
}

type UserMessageGroup struct {
	ID      string          `json:"id"`
	Name    string          `json:"name"`
	Avatar  string          `json:"avatar"`
	History []*GroupHistory `json:"history"`
	Members []*GroupMembers `json:"members"`
}

type WebSocketMessage struct {
	GroupId string      `json:"group_id"`
	Type    string      `json:"type"`
	Data    interface{} `json:"data"`
}

var AllGroup map[string]*UserMessageGroup
var AllUsers map[string]*User
var UserAvatar []string

/*
所有用户
用户有哪些群
	群里有哪些成员
	群里历史消息
用户有哪些好友
*/

const StaticAddress = "http://127.0.0.1:34332"

func InitData() {
	AllGroup = make(map[string]*UserMessageGroup)
	AllUsers = make(map[string]*User)
	GroupAvatar, err := os.ReadDir("./static/group_avatar/")
	if err != nil {
		log.Println("读取目录失败: ", err)
		return
	}

	for _, entry := range GroupAvatar {
		groupUuid := uuid.New().String()
		AllGroup[groupUuid] = &UserMessageGroup{
			ID:      groupUuid,
			Name:    groupUuid,
			Avatar:  StaticAddress + "/avatar/group_avatar/" + entry.Name(),
			History: nil,
			Members: nil,
		}

	}

	userAvatar, err := os.ReadDir("./static/user_avatar/")
	if err != nil {
		log.Println("读取目录失败: ", err)
		return
	}

	for _, entry := range userAvatar {
		UserAvatar = append(UserAvatar, StaticAddress+"/avatar/user_avatar/"+entry.Name())
		userUuid := uuid.New().String()

		userData := &User{
			Id:     userUuid,
			Name:   userUuid,
			Avatar: StaticAddress + "/avatar/user_avatar/" + entry.Name(),
			Conn:   nil,
		}
		AllUsers[userUuid] = userData
	}
	//
	//for k := range AllUsers {
	//	for j := range AllGroup[k] {
	//		AllGroup[k][j].Members = groupUserList
	//	}
	//}

	//log.Println(ActiveGroup)
	//log.Println(ActiveUsers)
	//for i := range AllGroup {
	//	log.Println(i)
	//}

}
