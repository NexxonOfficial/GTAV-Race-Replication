class Lobby {
    constructor() {
        this.Init()
        this.columns = []
        this.columnSelected = 0
        this.indexSelected = 0
    }

    Init() {
        this.InitStyleSheet()

        let lobbyMain = document.createElement('section')
        lobbyMain.classList.add('lobby-main')

        document.body.appendChild(lobbyMain)
        this.LobbyMain = lobbyMain
        return lobbyMain
    }

    CreateTitleAndDescription(title, subtitle) {
        let titleContainer = document.createElement('div')
        titleContainer.classList.add('title-container')
        
        let titleEl = document.createElement('h2')
        titleEl.innerHTML = title

        let subtitleEl = document.createElement('p')
        subtitleEl.innerHTML = subtitle

        titleContainer.appendChild(titleEl)
        titleContainer.appendChild(subtitleEl)

        this.LobbyMain.insertBefore(titleContainer, this.LobbyMain.firstChild)
    }

    CreateColumn(headerName, span) {
        let columnContainer = this.LobbyMain.getElementsByClassName('column-container')[0]

        if(this.LobbyMain.getElementsByClassName('column-container').length == 0) {
            columnContainer = document.createElement('div')
            columnContainer.classList.add('column-container')

            this.LobbyMain.appendChild(columnContainer)
        }

        let columnOverall = document.createElement('div')
        columnOverall.classList.add('column')

        if(span !== 1) {
            columnOverall.style.flex = span
        }

        let columnHeader = document.createElement('div')
        columnHeader.classList.add('column-header')

        let headerText = document.createElement('p')
        headerText.innerHTML = headerName

        columnHeader.appendChild(headerText)

        columnOverall.appendChild(columnHeader)

        columnContainer.appendChild(columnOverall)

        let newColumn = new Column(columnOverall)
        this.columns.push(newColumn)
        return newColumn
    }

    InitStyleSheet() {
        return new Promise((res, rej) => {
            let link = document.createElement('link')
            link.href = 'lobby/lobby.css'
            link.rel = 'stylesheet'

            document.head.appendChild(link)
            res(true)
        })
    }

    Visible(toggle, useTransition) {
        if(toggle) {
            this.LobbyMain.style.display = 'flex'
            setTimeout(() => {
                this.LobbyMain.style.opacity = 1
                setTimeout(() => {
                    this.InitialSelection()
                }, 100);
            }, 100);
        } else {
            this.LobbyMain.style.opacity = 0
            setTimeout(() => {
                this.LobbyMain.style.display = 'none'
            }, useTransition ? 501 : 1);
        }
    }

    InitialSelection() {
        if(this.columns.length !== 0) {
            // setting all dataset index values to their position in the lobby
            for(let i=0; i < this.columns.length; i++) {
                this.columns[i].element.dataset.index = i
            }

            let firstItem = null
            for(let i=1; i < this.columns[0].element.children.length; i++) {
                if(this.columns[0].element.childNodes[i].dataset.clickAction) {
                    firstItem = this.columns[0].element.childNodes[i]
                    break
                }
            }

            if(firstItem !== null) {
                let styleBeforeOpacity = firstItem.style.background
                let stripped = styleBeforeOpacity.replace('rgba(', '').replace(', 0.7)', '')
                firstItem.style.background = `rgb(${stripped}, 1)`
                this.indexSelected = 0
                this.columnSelected = this.columns[0].element
                this.ButtonSelected = firstItem
                this.HandleDescription(firstItem)
            } else {
                throw new Error('There was no button present in the first column with an onClickFunction parameter. Try adding one to allow users to navigate your menu!')
            }
        } else {
            throw new Error('There was no column to select a default button for. Add a column to get started!')
        }
    }

    HandleDescription(element) {
        if(element.dataset.description) {
            let descriptionElement = document.createElement('div')
            descriptionElement.classList.add('description-overall')
            descriptionElement.innerHTML = element.dataset.description

            let descriptionIcon = document.createElement('img')
            descriptionIcon.src = './img/radar_info_icon.png'
            descriptionElement.appendChild(descriptionIcon)

            this.columns[0].element.appendChild(descriptionElement)
        }
    }

    HandleControlPressDown() {
        let column = this.columnSelected
        let index = this.indexSelected

        if(this.columns.length !== 0) {
            let useableButtons = []
            for(let i=1; i < column.children.length; i++) {
                if(column.childNodes[i].dataset.clickAction) {
                    useableButtons.push(column.childNodes[i])
                }
            }

            if(index < (useableButtons.length - 1)) {
                index++
                this.indexSelected = index
            } else {
                index = 0
                this.indexSelected = 0
            }

            let buttonFrom = useableButtons[index == 0 ? useableButtons.length - 1 : index - 1]
            let buttonNow = useableButtons[index]

            buttonFrom.style.background = buttonFrom.style.background.replace(`)`, `, 0.7)`)

            buttonNow.style.background = buttonNow.style.background.replace(`0.7`, `1`)

            if(document.getElementsByClassName("description-overall")[0]) {
                column.removeChild(document.getElementsByClassName("description-overall")[0])
            }

            this.HandleDescription(buttonNow)

            this.ButtonSelected = buttonNow
        }
    }

    HandleControlPressUp() {
        let column = this.columnSelected
        let index = this.indexSelected

        if(this.columns.length !== 0) {
            let useableButtons = []
            for(let i=1; i < column.children.length; i++) {
                if(column.childNodes[i].dataset.clickAction) {
                    useableButtons.push(column.childNodes[i])
                }
            }

            if(index !== 0) {
                index--
                this.indexSelected = index
            } else {
                index = (useableButtons.length - 1)
                this.indexSelected = index
            }

            let buttonFrom = useableButtons[index == (useableButtons.length - 1) ? 0 : (index + 1)]
            let buttonNow = useableButtons[index]

            buttonFrom.style.background = buttonFrom.style.background.replace(`)`, `, 0.7)`)

            buttonNow.style.background = buttonNow.style.background.replace(`0.7`, `1`)

            if(document.getElementsByClassName("description-overall")[0]) {
                column.removeChild(document.getElementsByClassName("description-overall")[0])
            }

            this.HandleDescription(buttonNow)

            this.ButtonSelected = buttonNow
        }
    }

    HandleControlPressRight() {
        let columnBefore = this.columnSelected

        let columnAfter = parseInt(columnBefore.dataset.index) + 1 < this.columns.length ? (parseInt(columnBefore.dataset.index) + 1) : 0

        if(this.ButtonSelected.classList.contains('list-item')) {
            let parsed = JSON.parse(this.ButtonSelected.dataset.list)
            let index = parseInt(this.ButtonSelected.dataset.index) + 1 == parsed.length ? 0 : parseInt(this.ButtonSelected.dataset.index) + 1
            this.ButtonSelected.getElementsByClassName('right')[0].innerHTML = " <span><</span> " + parsed[index].ListName + " <span>></span> "
            this.ButtonSelected.dataset.index = index

            this.HandleListChange(parsed[index].ListName)
            return
        }

        if(this.columns.length > 1) {
            // find if column has any selectable buttons
            let useableButtonsBeforeColumn = []
            for(let i=1; i < columnBefore.children.length; i++) {
                if(columnBefore.childNodes[i].dataset.clickAction) {
                    useableButtonsBeforeColumn.push(columnBefore.childNodes[i])
                }
            }
            let useableButtonsAfterColumn = []
            for(let i=1; i < this.columns[columnAfter].element.children.length; i++) {
                if(this.columns[columnAfter].element.childNodes[i].dataset.clickAction) {
                    useableButtonsAfterColumn.push(this.columns[columnAfter].element.childNodes[i])
                }
            }

            if(useableButtonsAfterColumn.length > 0) {
                let buttonFrom = this.ButtonSelected !== undefined ? this.ButtonSelected : this.columns[0].element.childNodes[1]
                buttonFrom.style.background = buttonFrom.style.background.replace(`)`, `, 0.7)`)

                let buttonNew = useableButtonsAfterColumn[0]
                buttonNew.style.background = buttonNew.style.background.replace(`0.7`, `1`)

                if(document.getElementsByClassName("description-overall")[0]) {
                    this.columns[0].element.removeChild(document.getElementsByClassName("description-overall")[0])
                }
    
                this.HandleDescription(buttonNew)

                this.ButtonSelected = buttonNew
                this.columnSelected = this.columns[columnAfter].element
                this.indexSelected = 0
            } else {
                this.columnSelected = this.columns[parseInt(columnBefore.dataset.index) + 1].element
                this.HandleControlPressRight()
            }
        } else {
            return
        }
    } 

    HandleControlPressLeft() {
        let columnBefore = this.columnSelected
        let columnAfter = parseInt(columnBefore.dataset.index) - 1 < 0 ? this.columns.length - 1 : parseInt(columnBefore.dataset.index) - 1

        if(this.ButtonSelected.classList.contains('list-item')) {
            let parsed = JSON.parse(this.ButtonSelected.dataset.list)
            let index = parseInt(this.ButtonSelected.dataset.index) - 1 == -1 ? parsed.length - 1 : parseInt(this.ButtonSelected.dataset.index) - 1
            this.ButtonSelected.getElementsByClassName('right')[0].innerHTML = " <span><</span> " + parsed[index].ListName + " <span>></span> "
            this.ButtonSelected.dataset.index = index

            this.HandleListChange(parsed[index].ListName)
            return
        }

        if(this.columns.length > 1) {
            // find if column has any selectable buttons
            let useableButtonsBeforeColumn = []
            for(let i=1; i < columnBefore.children.length; i++) {
                if(columnBefore.childNodes[i].dataset.clickAction) {
                    useableButtonsBeforeColumn.push(columnBefore.childNodes[i])
                }
            }
            let useableButtonsAfterColumn = []
            for(let i=1; i < this.columns[columnAfter].element.children.length; i++) {
                if(this.columns[columnAfter].element.childNodes[i].dataset.clickAction) {
                    useableButtonsAfterColumn.push(this.columns[columnAfter].element.childNodes[i])
                }
            }

            if(useableButtonsAfterColumn.length > 0) {
                let buttonFrom = this.ButtonSelected !== undefined ? this.ButtonSelected : this.columns[0].element.childNodes[1]
                buttonFrom.style.background = buttonFrom.style.background.replace(`)`, `, 0.7)`)

                let buttonNew = useableButtonsAfterColumn[0]
                buttonNew.style.background = buttonNew.style.background.replace(`0.7`, `1`)

                if(document.getElementsByClassName("description-overall")[0]) {
                    this.columns[0].element.removeChild(document.getElementsByClassName("description-overall")[0])
                }
    
                this.HandleDescription(buttonNew)

                this.ButtonSelected = buttonNew
                this.columnSelected = this.columns[columnAfter].element
                this.indexSelected = 0
            } else {
                this.columnSelected = this.columns[columnAfter].element
                this.HandleControlPressLeft()
            }
        } else {
            return
        }
    } 

    HandleControlPressEnter() {
        let button = this.ButtonSelected
        fetch(`https://${GetParentResourceName()}/lobbyButtonPressed`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json; charset=UTF-8',
            },
            body: JSON.stringify({
                buttonAction: button.dataset.clickAction
            })
        }).then(async (response) => {
            await response.json().then((res) => {
                switch(res.action) {
                    case 1:
                        let left = button.getElementsByClassName('left')[0]
                        left.innerHTML == 'Ready Up' ? left.innerHTML = 'Unready' : left.innerHTML = 'Ready Up'
                        break
                    case 2:
                        let confirm = this.LobbyMain.getElementsByClassName('selection-text')[0].getElementsByTagName('h2')[0]
                        confirm.style.backgroundColor = 'rgb(105, 187, 104)'
                        confirm.innerHTML = 'CONFIRMED'
                        button.getElementsByClassName('left')[0].innerHTML = 'Confirmed'
                        button.dataset.clickAction = 0
                        if(this.columns[0].element.getElementsByClassName('list-item')[0]) {
                            this.columns[0].element.removeChild(this.columns[0].element.getElementsByClassName('list-item')[0])
                        }
                        break
                }
            }).catch((err) => {
                throw new Error(err)
            })
        }).catch((err) => {
            throw new Error(err)
        })
    }
    
    HandleListChange(value) {
        fetch(`https://${GetParentResourceName()}/lobbyListChange`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json; charset=UTF-8',
            },
            body: JSON.stringify({
                value: value
            })
        }).then(async (response) => {
            await response.json().then((res) => {
                
            }).catch((err) => {
                throw new Error(err)
            })
        }).catch((err) => {
            throw new Error(err)
        })
    }
}