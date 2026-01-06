package api

import (
	"github.com/google/uuid"
	"github.com/gorilla/websocket"
	"log"
	"os"
)

type User struct {
	Id     string          `json:"id"`
	Name   string          `json:"name"`
	Avatar string          `json:"avatar"`
	Conn   *websocket.Conn `json:"-"`
}

type GroupHistoryMessage struct {
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
	ID      string                 `json:"id"`
	Name    string                 `json:"name"`
	Avatar  string                 `json:"avatar"`
	History []*GroupHistoryMessage `json:"history"`
	Members []*GroupMembers        `json:"members"`
}

var ActiveGroup map[string][]*UserMessageGroup
var ActiveUsers map[string]*User
var AllUsers map[string]*User

/*
所有用户
用户有哪些群
	群里有哪些成员
	群里历史消息
用户有哪些好友
*/

func InitData() {
	ActiveGroup = make(map[string][]*UserMessageGroup)
	ActiveUsers = make(map[string]*User)
	AllUsers = make(map[string]*User)
	GroupAvatar, err := os.ReadDir("./static/group_avatar/")
	if err != nil {
		log.Println("读取目录失败: ", err)
		return
	}

	var groupList []*UserMessageGroup
	for _, entry := range GroupAvatar {
		groupUuid := uuid.New().String()
		groupData := &UserMessageGroup{
			ID:      groupUuid,
			Name:    groupUuid,
			Avatar:  "http://127.0.0.1:34332/avatar/group_avatar/" + entry.Name(),
			History: nil,
			Members: nil,
		}
		groupList = append(groupList, groupData)
	}

	UserAvatar, err := os.ReadDir("./static/user_avatar/")
	if err != nil {
		log.Println("读取目录失败: ", err)
		return
	}
	var groupUserList []*GroupMembers
	for _, entry := range UserAvatar {
		userUuid := uuid.New().String()

		userData := &User{
			Id:     userUuid,
			Name:   userUuid,
			Avatar: "http://127.0.0.1:34332/avatar/user_avatar/" + entry.Name(),
			Conn:   nil,
		}
		AllUsers[userUuid] = userData
		groupUserData := &GroupMembers{
			Id:       userData.Id,
			Name:     userData.Id,
			Avatar:   "http://127.0.0.1:34332/avatar/user_avatar/" + entry.Name(),
			Usertype: "普通群员",
			Status:   "正常",
		}
		ActiveUsers[userUuid] = userData
		groupUserList = append(groupUserList, groupUserData)

		ActiveGroup[userUuid] = groupList

	}

	for k := range ActiveUsers {
		for j := range ActiveGroup[k] {
			ActiveGroup[k][j].Members = groupUserList
		}
	}

	//log.Println(ActiveGroup)
	//log.Println(ActiveUsers)
	for i := range ActiveGroup {
		log.Println(i)
	}

}
