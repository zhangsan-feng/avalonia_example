package user

import (
	"avalonia_server/api/datastore"
	"github.com/gin-gonic/gin"
	"log"
	"strings"
)

type SearchParams struct {
	Keyword string `json:"keyword" form:"keyword" binding:"required"`
}

type SearchResultGroup struct {
	ID      string                    `json:"id"`
	Name    string                    `json:"name"`
	Avatar  string                    `json:"avatar"`
	Type    string                    `json:"type"`
	Members []*datastore.GroupMembers `json:"members"`
}

type SearchResultUser struct {
	ID     string `json:"id"`
	Name   string `json:"name"`
	Avatar string `json:"avatar"`
	Status string `json:"status"`
}

type SearchResponse struct {
	Groups []*SearchResultGroup `json:"groups"`
	Users  []*SearchResultUser  `json:"users"`
}

func SearchGroupAndUserApi(r *gin.Context) {
	params := &SearchParams{}
	if bindError := r.ShouldBind(params); bindError != nil {
		log.Println(bindError)
		r.JSON(400, gin.H{
			"code": 400,
			"msg":  "参数错误",
			"data": nil,
		})
		return
	}

	result := &SearchResponse{
		Groups: []*SearchResultGroup{},
		Users:  []*SearchResultUser{},
	}

	keyword := strings.ToLower(params.Keyword)

	for _, group := range datastore.AllGroup {
		groupName := strings.ToLower(group.Name)
		if strings.Contains(groupName, keyword) {
			result.Groups = append(result.Groups, &SearchResultGroup{
				ID:      group.ID,
				Name:    group.Name,
				Avatar:  group.Avatar,
				Type:    group.Type,
				Members: group.Members,
			})
		}
	}

	for userId, user := range datastore.AllUsers {
		userName := strings.ToLower(user.Name)
		if strings.Contains(userName, keyword) {
			result.Users = append(result.Users, &SearchResultUser{
				ID:     userId,
				Name:   user.Name,
				Avatar: user.Avatar,
				Status: user.Status,
			})
		}
	}
	
	r.JSON(200, gin.H{
		"code": 200,
		"msg":  "success",
		"data": result,
	})
}
