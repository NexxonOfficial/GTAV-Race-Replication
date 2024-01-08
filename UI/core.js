window.onload = () => {
    let UIShowing = null

    function dummy() {

    }

    window.addEventListener('message', (event) => {
        switch(event.data.type) {
            case 'controlpress':
                HandleControlPress(event.data.control)
                break
            case 'baseCreate':
                HandleBaseCreate(event.data.baseType)
                break
            case 'createTitleAndSubtitle':
                HandleBaseTitleAndSubtitleCreate(event.data.title, event.data.subtitle)
                break
            case 'createColumn':
                HandleColumnCreation(event.data.headername, event.data.span)
                break
            case 'createDetailItem':
                HandleColumnDetailItemCreation(event.data.column, event.data.leftText, event.data.rightText, event.data.description, event.data.color, event.data.seperator, event.data.closeGap, event.data.onClickFunction)
                break
            case 'createListItem':
                HandleColumnListItemCreation(event.data.column, event.data.leftText, event.data.list, event.data.description, event.data.color, event.data.seperator, event.data.closeGap, event.data.onClickFunction)
                break
            case 'createPlayerItem':
                HandleColumnPlayerItemCreation(event.data.column, event.data.name, event.data.level, event.data.statusText, event.data.statusColor, event.data.crewTag, event.data.color, event.data.onClickFunction)
                break
            case 'createSelectionTextItem':
                HandleColumnSelectionTextItemCreation(event.data.column, event.data.topText, event.data.bottomText, event.data.isSelected)
                break
            case 'createDetailImageItem':
                HandleColumnDetailImageItemCreation(event.data.column, event.data.imgSource, event.data.imgTitle)
                break
            case 'updateSelectionTextItemText':
                HandleUpdateSelectionTextItemText(event.data.column, event.data.topText, event.data.bottomText)
                break
            case 'baseVisible':
                HandleBaseVisible(event.data.useTransition)
                break
            case 'baseDispose':
                HandleBaseDispose(event.data.useTransition)
                break
            case 'updatePlayerStatus':
                HandlePlayerStatusChange(event.data.playerName, event.data.statusText, event.data.statusColor)
                break
            default: break;
        }
    })

    function HandleControlPress(control) {
        switch(control) {
            case 32 || 172:
                UIShowing.HandleControlPressUp()
                break
            case 33 || 173:
                UIShowing.HandleControlPressDown()
                break
            case 34 || 174:
                UIShowing.HandleControlPressLeft()
                break
            case 35 || 175:
                UIShowing.HandleControlPressRight()
                break
            case 177:
                UIShowing.HandleControlPressBack()
                break
            case 191:
                UIShowing.HandleControlPressEnter()
                break
        }
    }

    function HandleBaseCreate(type) {
        switch(type) {
            case 1:
                UIShowing = new Lobby()
                break
        }
    }

    function HandleBaseVisible(useTransition) {
        UIShowing.Visible(true, useTransition)
    }

    function HandleColumnCreation(headerName, span) {
        UIShowing.CreateColumn(headerName, span)
    }

    function HandleBaseTitleAndSubtitleCreate(title, subtitle) {
        UIShowing.CreateTitleAndDescription(title, subtitle)
    }

    function HandleColumnDetailItemCreation(columnIndex, leftText, rightText, description, color, seperator, closeGap, onClickFunction) {
        UIShowing.columns[columnIndex].CreateDetailItem(leftText, rightText, description, color, seperator, closeGap, onClickFunction)
    }

    function HandleColumnListItemCreation(columnIndex, leftText, list, description, color, seperator, closeGap, onClickFunction) {
        UIShowing.columns[columnIndex].CreateListItem(leftText, list, description, color, seperator, closeGap, onClickFunction)
    }

    function HandleColumnPlayerItemCreation(columnIndex, name, level, statusText, statusColor, crewTag, color, onClickFunction) {
        UIShowing.columns[columnIndex].CreatePlayerItem(name, level, statusText, statusColor, crewTag, color, onClickFunction)
    }

    function HandleColumnDetailImageItemCreation(columnIndex, imgSource, imgTitle) {
        UIShowing.columns[columnIndex].CreateDetailImageItem(imgSource, imgTitle)
    }

    function HandleColumnSelectionTextItemCreation(columnIndex, topText, bottomText, isSelected) {
        UIShowing.columns[columnIndex].CreateSelectionTextItem(topText, bottomText, isSelected)
    }

    function HandleUpdateSelectionTextItemText(columnIndex, topText, bottomText) {
        UIShowing.columns[columnIndex].UpdateSelectionTextItemText(topText, bottomText)
    }

    function HandlePlayerStatusChange(playerName, statusText, statusColor) {
        let column = null
        for(let i=0; i < UIShowing.columns.length; i++) {
            if(UIShowing.columns[i].element.getElementsByClassName('column-header')[0].innerHTML.includes('PLAYERS')) {
                column = UIShowing.columns[i].element
                break
            }
        }

        if(column !== null) {
            let playerNodes = column.getElementsByClassName('player-item')
            for(let i=0; i < playerNodes.length; i++) {
                if(playerNodes[i].getElementsByTagName('p')[0].innerHTML == playerName) {
                    let status = playerNodes[i].getElementsByTagName('p')[1]
                    status.innerHTML = statusText
                    if(statusColor !== null) {
                        status.style.backgroundColor = statusColor
                    }

                    if(statusText == 'LEFT') {
                        column.appendChild(playerNodes[i])
                        setTimeout(() => {
                            // Position can change within the 3 seconds, and will delete the wrong one of not searched for again.
                            for(let i2=0; i2 < playerNodes.length; i2++) {
                                if(playerNodes[i2].getElementsByTagName('p')[0].innerHTML == playerName) {
                                    column.removeChild(playerNodes[i2])
                                }
                            }
                        }, 3000);
                    }
                }
            }
        } else {
            throw new Error('No players column found. Please use PLAYERS in the column name to be established by the script.')
        }
    }

    function HandleBaseDispose(useTransition) {
        UIShowing.Visible(false, useTransition)
        setTimeout(() => {
            document.body.removeChild(document.body.getElementsByClassName('lobby-main')[0])
            UIShowing = null
        }, useTransition ? 501 : 1);
    }
}