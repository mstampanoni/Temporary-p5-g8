using UnityEngine;

public class RotatingSelection : MonoBehaviour
{
    #region Init Value
    [Header("Références")]
    [SerializeField] private RectTransform mAttackCircle; 
    [SerializeField] private RectTransform mCompetenceCircle; 

    [Header("Paramètres d'animation")]
    [SerializeField] private float mSelectedScale = 1.5f; 
    [SerializeField] private float mDeselectedScale = 1f; 
    [SerializeField] private float mRotationSpeed = 100f; 
    [SerializeField] private float mAnimationSpeed = 0.2f; 

    private RectTransform mCurrentSelected; 
    private RectTransform mPreviousSelected;

    private RectTransform mCurrentConfirmSelected;


    private Vector3 mOriginalScale;

    private bool mIsAnimating = false;
    #endregion

    private void Start()
    {
        mCurrentSelected = mAttackCircle;
        mPreviousSelected = mCompetenceCircle;
        mCurrentConfirmSelected = null;

        mOriginalScale = mAttackCircle.localScale;

        UpdateSelection();
    }

    public void Select(RectTransform newSelection)
    {
        if (mIsAnimating)
            return;

        if (newSelection == mCurrentSelected && mCurrentConfirmSelected == null)
        {
            mCurrentConfirmSelected = newSelection;
        }
        else if (newSelection != mCurrentSelected)
        {
            if (mCurrentConfirmSelected != null)
            {
                mCurrentConfirmSelected = null;
            }
            mPreviousSelected = mCurrentSelected;
            mCurrentSelected = newSelection;
        }
       
        UpdateSelection();
    }

    private void UpdateSelection()
    {
        StopAllCoroutines();

        StartCoroutine(AnimateScale(mPreviousSelected, mDeselectedScale));
        StartCoroutine(AnimateScale(mCurrentSelected, mSelectedScale));

        StartCoroutine(RotateSelected(mCurrentSelected.parent));
    }

    private System.Collections.IEnumerator AnimateScale(RectTransform target, float targetScale)
    {
        mIsAnimating = true;

        target.localScale = mOriginalScale;
        Vector3 newScale = new Vector3(target.localScale.x * targetScale, target.localScale.y * targetScale, target.localScale.z);
        float elapsedTime = 0f;

        while (elapsedTime < mAnimationSpeed)
        {
            elapsedTime += Time.deltaTime;
            target.localScale = Vector3.Lerp(target.localScale, newScale, elapsedTime / mAnimationSpeed);
            yield return null;
        }

        mIsAnimating = false;
    }

    private System.Collections.IEnumerator RotateSelected(Transform parent)
    {
        while (mCurrentSelected.parent == parent)
        {
            parent.Rotate(0f, 0f, -mRotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    #region Getter
    public RectTransform GetSelected()
    {
        return mCurrentSelected;
    }

    public RectTransform GetConfirmed()
    {
        return mCurrentConfirmSelected;
    }

    public RectTransform GetAttackCircle()
    {
        return mAttackCircle;
    }

    public RectTransform GetCompetenceCircle()
    {
        return mCompetenceCircle;
    }
    #endregion

    #region Setter
    public void setConfirmedActionNull()
    {
        mCurrentConfirmSelected = null;
    }
    #endregion
}
