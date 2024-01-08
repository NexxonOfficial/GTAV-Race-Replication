class MainMenu {
    constructor() {
        this.Init()
    }

    Init() {
        let overall = document.createElement('div')
        overall.classList.add('main-menu-overall')

        this.Overall = overall
        this.CreateContainer()
        document.body.appendChild(overall)
    }

    CreateContainer() {
        let container = document.createElement('div')
        container.classList.add('main-menu-container')

        let left = document.createElement('div')
        left.classList.add('left')
        this.InitLeft(left)

        let right = document.createElement('div')
        right.classList.add('right')

        container.appendChild(left)
        container.appendChild(right)

        this.Overall.appendChild(container)
    }

    InitLeft(left) {
        let menuHeader = document.createElement('h1')
        menuHeader.classList.add('menu-header')
        menuHeader.innerHTML = 'Main Menu'

        left.appendChild(menuHeader)
        this.InitMainSelector(left)
        this.InitOptionSelector(left)
    }

    InitMainSelector(left) {
        let mainSelector = document.createElement('div')
        mainSelector.classList.add('main-menu-main-selector')

        this.CreateMainItem(mainSelector, 'Play Prop Hunt', 'Transform, Hide, Seek: The Thrilling Pursuit of Disguise and Discovery in GTA 5 Prop Hunt. Question is, will you survive the full time?', '../img/test.jpg')

        left.appendChild(mainSelector)
    }

    CreateMainItem(selector, title, description, bg) {
        let mainItem = document.createElement('div')
        mainItem.classList.add('main-item')

        mainItem.style.backgroundImage = `url(${bg})`
        mainItem.style.backgroundPosition = 'center'
        mainItem.style.backgroundRepeat = 'no-repeat'
        mainItem.style.backgroundSize = 'cover'

        let mainHeaderContainer = document.createElement('div')
        mainHeaderContainer.classList.add('text-container')

        let mainHeader = document.createElement('h2')
        mainHeader.classList.add('header')
        mainHeader.innerHTML = title

        let mainSub = document.createElement('p')
        mainSub.classList.add('sub-header')
        mainSub.innerHTML = description

        mainHeaderContainer.appendChild(mainHeader)
        mainHeaderContainer.appendChild(mainSub)

        mainItem.appendChild(mainHeaderContainer)

        selector.appendChild(mainItem)
    }

    InitOptionSelector(left) {
        let optionSelector = document.createElement('div')
        optionSelector.classList.add('main-menu-option-selector')

        this.CreateOptionBox(optionSelector, 'Store', '../img/test.jpg')
        this.CreateOptionBox(optionSelector, 'Settings', '../img/test.jpg')
        this.CreateOptionBox(optionSelector, 'Statistics', '../img/test.jpg')

        left.appendChild(optionSelector)
    }

    CreateOptionBox(optionSelector, title, img) {
        let option = document.createElement('div')
        option.classList.add('option-main')

        let optionHeader = document.createElement('h3')
        optionHeader.innerHTML = title

        option.style.background = `url(${img})`
        option.style.backgroundPosition = 'center'
        option.style.backgroundSize = 'cover'
        option.style.backgroundRepeat = 'no-repeat'

        option.appendChild(optionHeader)
        
        optionSelector.appendChild(option)
    }
}